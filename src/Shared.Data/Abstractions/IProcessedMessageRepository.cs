using Shared.Models.Messaging;

namespace Shared.Data.Abstractions;

public interface IProcessedMessageRepository
{
    Task<bool> HasBeenProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken);
    Task MarkAsProcessedAsync(ProcessedMessage message, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
