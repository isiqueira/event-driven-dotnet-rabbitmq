using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Events;
using Shared.Messaging;

namespace Order.Api.Messaging;

public sealed class RabbitMqOrderEventPublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqOrderEventPublisher> logger)
    : IOrderEventPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task PublishAsync(
        OrderCreatedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var factory = CreateConnectionFactory();
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(
            integrationEvent,
            IntegrationEventJson.SerializerOptions);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = integrationEvent.EventId.ToString("D"),
            CorrelationId = integrationEvent.CorrelationId,
            Type = integrationEvent.EventType,
            Timestamp = new AmqpTimestamp(integrationEvent.OccurredAt.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                ["x-correlation-id"] = integrationEvent.CorrelationId,
                ["x-retry-count"] = 0
            }
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: _options.OrderCreatedRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Published {EventType} event {EventId} for order {OrderId} with correlation ID {CorrelationId}",
            integrationEvent.EventType,
            integrationEvent.EventId,
            integrationEvent.OrderId,
            integrationEvent.CorrelationId);
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
