namespace Inventory.Worker.Models;

public sealed class ProcessedMessage
{
    private ProcessedMessage()
    {
    }

    public ProcessedMessage(
        Guid eventId,
        string eventType,
        string consumer,
        DateTimeOffset processedAt,
        string correlationId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        EventType = eventType;
        Consumer = consumer;
        ProcessedAt = processedAt;
        CorrelationId = correlationId;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Consumer { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
}
