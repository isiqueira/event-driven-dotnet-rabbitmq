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

        foreach (var endpoint in RabbitMqEventRouting.GetAllEndpoints(options))
        {
            await DeclareEndpointAsync(channel, options, endpoint, cancellationToken);
        }
    }

    private static async Task DeclareEndpointAsync(
        IChannel channel,
        RabbitMqOptions options,
        RabbitMqEventEndpoint endpoint,
        CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(
            queue: endpoint.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: endpoint.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = options.RetryDelayMilliseconds,
                ["x-dead-letter-exchange"] = options.ExchangeName,
                ["x-dead-letter-routing-key"] = endpoint.RoutingKey
            },
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: endpoint.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: endpoint.Queue,
            exchange: options.ExchangeName,
            routingKey: endpoint.RoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: endpoint.RetryQueue,
            exchange: options.ExchangeName,
            routingKey: endpoint.RetryRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: endpoint.DeadLetterQueue,
            exchange: options.ExchangeName,
            routingKey: endpoint.DeadLetterRoutingKey,
            cancellationToken: cancellationToken);
    }
}
