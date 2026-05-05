namespace Shared.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "app";
    public string Password { get; init; } = "app";
    public string ExchangeName { get; init; } = "orders.exchange";
    public string OrderCreatedQueue { get; init; } = "orders.created.queue";
    public string OrderCreatedRetryQueue { get; init; } = "orders.created.retry.queue";
    public string OrderCreatedDeadLetterQueue { get; init; } = "orders.created.dlq";
    public string OrderCreatedRoutingKey { get; init; } = "orders.created";
    public string OrderCreatedRetryRoutingKey { get; init; } = "orders.created.retry";
    public string OrderCreatedDeadLetterRoutingKey { get; init; } = "orders.created.dead-letter";
    public int MaxRetryCount { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 5000;
}
