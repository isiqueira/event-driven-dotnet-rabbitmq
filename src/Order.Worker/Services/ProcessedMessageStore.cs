using Microsoft.EntityFrameworkCore;
using Order.Worker.Data;
using Order.Worker.Models;
using Shared.Events;

namespace Order.Worker.Services;

public sealed class ProcessedMessageStore(WorkerDbContext dbContext)
{
    public Task<bool> HasProcessedAsync(
        Guid eventId,
        string eventType,
        string consumer,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProcessedMessages.AnyAsync(
            message => message.EventId == eventId
                       && message.EventType == eventType
                       && message.Consumer == consumer,
            cancellationToken);
    }

    public async Task MarkProcessedAsync(
        OrderCreatedEvent integrationEvent,
        string consumer,
        CancellationToken cancellationToken = default)
    {
        dbContext.ProcessedMessages.Add(new ProcessedMessage(
            integrationEvent.EventId,
            integrationEvent.EventType,
            consumer,
            DateTimeOffset.UtcNow,
            integrationEvent.CorrelationId));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
