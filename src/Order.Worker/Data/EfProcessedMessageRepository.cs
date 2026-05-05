using Microsoft.EntityFrameworkCore;
using Shared.Data.Abstractions;
using Shared.Models.Messaging;

namespace Order.Worker.Data;

public sealed class EfProcessedMessageRepository(OrderWorkerDbContext dbContext) : IProcessedMessageRepository
{
    public Task<bool> HasBeenProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken)
    {
        return dbContext.ProcessedMessages.AnyAsync(
            message => message.EventId == eventId && message.Consumer == consumer,
            cancellationToken);
    }

    public async Task MarkAsProcessedAsync(ProcessedMessage message, CancellationToken cancellationToken)
    {
        await dbContext.ProcessedMessages.AddAsync(message, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
