namespace Shared.Events;

public sealed record InventoryReservationFailedEvent(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    Guid OrderId,
    string Reason,
    OrderCreatedItem[] Items)
    : IntegrationEvent(EventId, EventType, OccurredAt, CorrelationId)
{
    public const string Name = "InventoryReservationFailed";
}
