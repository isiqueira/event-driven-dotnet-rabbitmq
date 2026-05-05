using System.Text.Json;
using Microsoft.Extensions.Options;
using Order.Worker.Messaging;
using Order.Worker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using Shared.Messaging;

namespace Order.Worker;

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

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) => await HandleMessageAsync(channel, args, stoppingToken);

        var consumerTag = await channel.BasicConsumeAsync(
            queue: _options.OrderCreatedQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "Order worker started consuming queue {QueueName} with consumer tag {ConsumerTag}",
            _options.OrderCreatedQueue,
            consumerTag);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Order worker is stopping");
        }
    }

    private async Task HandleMessageAsync(
        IChannel channel,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        var body = args.Body.ToArray();
        var retryCount = RabbitMqHeaderReader.GetRetryCount(args.BasicProperties.Headers);
        var correlationId = args.BasicProperties.CorrelationId ?? RabbitMqHeaderReader.GetCorrelationId(args.BasicProperties.Headers);

        try
        {
            var integrationEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(
                body,
                IntegrationEventJson.SerializerOptions);

            if (integrationEvent is null)
            {
                throw new InvalidOperationException("OrderCreated message body could not be deserialized.");
            }

            logger.LogInformation(
                "Consumed {EventType} event {EventId} for order {OrderId} with correlation ID {CorrelationId}",
                integrationEvent.EventType,
                integrationEvent.EventId,
                integrationEvent.OrderId,
                integrationEvent.CorrelationId);

            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<OrderCreatedMessageHandler>();
            await handler.HandleAsync(integrationEvent, cancellationToken);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to process delivery {DeliveryTag} with correlation ID {CorrelationId}",
                args.DeliveryTag,
                correlationId);

            await HandleProcessingFailureAsync(channel, args, body, retryCount, correlationId, cancellationToken);
        }
    }

    private async Task HandleProcessingFailureAsync(
        IChannel channel,
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
                await PublishAsync(
                    channel,
                    body,
                    args.BasicProperties,
                    _options.OrderCreatedRetryRoutingKey,
                    nextRetryCount,
                    cancellationToken);

                logger.LogWarning(
                    "Published retry attempt {RetryCount}/{MaxRetryCount} for delivery {DeliveryTag} with correlation ID {CorrelationId}",
                    nextRetryCount,
                    _options.MaxRetryCount,
                    args.DeliveryTag,
                    correlationId);
            }
            else
            {
                await PublishAsync(
                    channel,
                    body,
                    args.BasicProperties,
                    _options.OrderCreatedDeadLetterRoutingKey,
                    retryCount,
                    cancellationToken);

                logger.LogError(
                    "Published delivery {DeliveryTag} to DLQ after {RetryCount} retries with correlation ID {CorrelationId}",
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
                "Could not republish failed delivery {DeliveryTag}; requeueing original message",
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
