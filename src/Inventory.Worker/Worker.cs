using System.Text.Json;
using Inventory.Worker.Messaging;
using Inventory.Worker.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using Shared.Messaging;

namespace Inventory.Worker;

public sealed class Worker(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<Worker> logger)
    : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = CreateConnectionFactory();
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await RabbitMqTopology.DeclareAsync(channel, _options, stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        await StartConsumerAsync<OrderCreatedEvent>(
            channel,
            RabbitMqEventRouting.GetEndpoint(_options, OrderCreatedEvent.Name),
            async (scope, integrationEvent, cancellationToken) =>
            {
                var handler = scope.ServiceProvider.GetRequiredService<OrderCreatedMessageHandler>();
                await handler.HandleAsync(integrationEvent, cancellationToken);
            },
            stoppingToken);

        await StartConsumerAsync<OrderProcessedEvent>(
            channel,
            RabbitMqEventRouting.GetEndpoint(_options, OrderProcessedEvent.Name),
            async (scope, integrationEvent, cancellationToken) =>
            {
                var handler = scope.ServiceProvider.GetRequiredService<OrderProcessedMessageHandler>();
                await handler.HandleAsync(integrationEvent, cancellationToken);
            },
            stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Inventory worker is stopping");
        }
    }

    private async Task StartConsumerAsync<TEvent>(
        IChannel channel,
        RabbitMqEventEndpoint endpoint,
        Func<IServiceScope, TEvent, CancellationToken, Task> handleAsync,
        CancellationToken cancellationToken)
        where TEvent : IntegrationEvent
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
            await HandleMessageAsync(channel, endpoint, args, handleAsync, cancellationToken);

        var consumerTag = await channel.BasicConsumeAsync(
            queue: endpoint.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Inventory worker started consuming {EventType} queue {QueueName} with consumer tag {ConsumerTag}",
            endpoint.EventType,
            endpoint.Queue,
            consumerTag);
    }

    private async Task HandleMessageAsync<TEvent>(
        IChannel channel,
        RabbitMqEventEndpoint endpoint,
        BasicDeliverEventArgs args,
        Func<IServiceScope, TEvent, CancellationToken, Task> handleAsync,
        CancellationToken cancellationToken)
        where TEvent : IntegrationEvent
    {
        var body = args.Body.ToArray();
        var retryCount = RabbitMqHeaderReader.GetRetryCount(args.BasicProperties.Headers);
        var correlationId = args.BasicProperties.CorrelationId ?? RabbitMqHeaderReader.GetCorrelationId(args.BasicProperties.Headers);

        try
        {
            var integrationEvent = JsonSerializer.Deserialize<TEvent>(
                body,
                IntegrationEventJson.SerializerOptions);

            if (integrationEvent is null)
            {
                throw new InvalidOperationException($"{endpoint.EventType} message body could not be deserialized.");
            }

            using var scope = scopeFactory.CreateScope();
            await handleAsync(scope, integrationEvent, cancellationToken);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to process {EventType} delivery {DeliveryTag} with correlation ID {CorrelationId}",
                endpoint.EventType,
                args.DeliveryTag,
                correlationId);

            await HandleProcessingFailureAsync(channel, endpoint, args, body, retryCount, correlationId, cancellationToken);
        }
    }

    private async Task HandleProcessingFailureAsync(
        IChannel channel,
        RabbitMqEventEndpoint endpoint,
        BasicDeliverEventArgs args,
        byte[] body,
        int retryCount,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (retryCount < _options.MaxRetryCount)
            {
                var nextRetryCount = retryCount + 1;
                await PublishAsync(channel, body, args.BasicProperties, endpoint.RetryRoutingKey, nextRetryCount, cancellationToken);

                logger.LogWarning(
                    "Published retry attempt {RetryCount}/{MaxRetryCount} for {EventType} delivery {DeliveryTag} with correlation ID {CorrelationId}",
                    nextRetryCount,
                    _options.MaxRetryCount,
                    endpoint.EventType,
                    args.DeliveryTag,
                    correlationId);
            }
            else
            {
                await PublishAsync(channel, body, args.BasicProperties, endpoint.DeadLetterRoutingKey, retryCount, cancellationToken);

                logger.LogError(
                    "Published {EventType} delivery {DeliveryTag} to DLQ after {RetryCount} retries with correlation ID {CorrelationId}",
                    endpoint.EventType,
                    args.DeliveryTag,
                    retryCount,
                    correlationId);
            }

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Could not republish failed {EventType} delivery {DeliveryTag}; requeueing original message",
                endpoint.EventType,
                args.DeliveryTag);

            await channel.BasicNackAsync(
                args.DeliveryTag,
                multiple: false,
                requeue: true,
                cancellationToken);
        }
    }

    private async Task PublishAsync(
        IChannel channel,
        byte[] body,
        IReadOnlyBasicProperties sourceProperties,
        string routingKey,
        int retryCount,
        CancellationToken cancellationToken)
    {
        var headers = RabbitMqHeaderReader.CopyHeaders(sourceProperties.Headers);
        headers["x-retry-count"] = retryCount;

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = sourceProperties.ContentType ?? "application/json",
            ContentEncoding = sourceProperties.ContentEncoding ?? "utf-8",
            MessageId = sourceProperties.MessageId,
            CorrelationId = sourceProperties.CorrelationId,
            Type = sourceProperties.Type,
            Timestamp = sourceProperties.Timestamp,
            Headers = headers
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };
    }
}
