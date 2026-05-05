using Inventory.Worker.Data;
using Inventory.Worker.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace Inventory.Worker.Services;

public sealed class ProcessedMessageStore(InventoryDbContext dbContext)
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
        IntegrationEvent integrationEvent,
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
