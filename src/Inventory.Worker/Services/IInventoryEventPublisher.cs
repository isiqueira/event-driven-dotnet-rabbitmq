using Shared.Events;

namespace Inventory.Worker.Services;

public interface IInventoryEventPublisher
{
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
