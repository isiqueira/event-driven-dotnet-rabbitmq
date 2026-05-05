namespace Shared.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    string CorrelationId);
