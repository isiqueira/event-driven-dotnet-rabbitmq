namespace Shared.Events;

public sealed record OrderFulfilledEvent(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    Guid OrderId,
    Guid ReservationId)
    : IntegrationEvent(EventId, EventType, OccurredAt, CorrelationId)
{
    public const string Name = "OrderFulfilled";
}
