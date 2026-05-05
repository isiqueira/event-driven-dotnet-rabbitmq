namespace Shared.Events;

public sealed record OrderCreatedEvent(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    Guid OrderId,
    string CustomerId,
    decimal TotalAmount)
    : IntegrationEvent(EventId, EventType, OccurredAt, CorrelationId)
{
    public const string Name = "OrderCreated";
}
