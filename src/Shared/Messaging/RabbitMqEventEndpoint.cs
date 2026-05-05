namespace Shared.Messaging;

public sealed record RabbitMqEventEndpoint(
    string EventType,
    string Queue,
    string RetryQueue,
    string DeadLetterQueue,
    string RoutingKey,
    string RetryRoutingKey,
    string DeadLetterRoutingKey);
