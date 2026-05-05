using RabbitMQ.Client;

namespace Shared.Messaging;

public static class RabbitMqTopology
{
    public static async Task DeclareAsync(
        IChannel channel,
        RabbitMqOptions options,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: options.OrderCreatedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: options.OrderCreatedRetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = options.RetryDelayMilliseconds,
                ["x-dead-letter-exchange"] = options.ExchangeName,
                ["x-dead-letter-routing-key"] = options.OrderCreatedRoutingKey
            },
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: options.OrderCreatedDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: options.OrderCreatedQueue,
            exchange: options.ExchangeName,
            routingKey: options.OrderCreatedRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: options.OrderCreatedRetryQueue,
            exchange: options.ExchangeName,
            routingKey: options.OrderCreatedRetryRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: options.OrderCreatedDeadLetterQueue,
            exchange: options.ExchangeName,
            routingKey: options.OrderCreatedDeadLetterRoutingKey,
            cancellationToken: cancellationToken);
    }
}
