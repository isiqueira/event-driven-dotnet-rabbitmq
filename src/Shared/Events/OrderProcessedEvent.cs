namespace Shared.Events;

public sealed record OrderProcessedEvent(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    Guid OrderId,
    Guid ReservationId,
    InventoryEventItem[] Items)
    : IntegrationEvent(EventId, EventType, OccurredAt, CorrelationId)
{
    public const string Name = "OrderProcessed";
}
