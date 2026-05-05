using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging;

namespace Order.Api.Messaging;

public sealed class RabbitMqInitializer(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqInitializer> logger)
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var factory = CreateConnectionFactory();
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await RabbitMqTopology.DeclareAsync(channel, _options, cancellationToken);
        logger.LogInformation("RabbitMQ topology initialized for exchange {ExchangeName}", _options.ExchangeName);
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
