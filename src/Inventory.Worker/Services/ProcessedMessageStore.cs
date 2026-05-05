using Shared.Data.Abstractions;
using Shared.Events;
using Shared.Models.Messaging;

namespace Inventory.Worker.Services;

public sealed class ProcessedMessageStore(IProcessedMessageRepository processedMessageRepository)
{
    public Task<bool> HasProcessedAsync(
        Guid eventId,
        string eventType,
        string consumer,
        CancellationToken cancellationToken = default)
    {
        return processedMessageRepository.HasBeenProcessedAsync(eventId, consumer, cancellationToken);
    }

    public async Task MarkProcessedAsync(
        IntegrationEvent integrationEvent,
        string consumer,
        CancellationToken cancellationToken = default)
    {
        await processedMessageRepository.MarkAsProcessedAsync(new ProcessedMessage(
            integrationEvent.EventId,
            integrationEvent.EventType,
            consumer,
            DateTimeOffset.UtcNow,
            integrationEvent.CorrelationId),
            cancellationToken);

        await processedMessageRepository.SaveChangesAsync(cancellationToken);
    }
}
