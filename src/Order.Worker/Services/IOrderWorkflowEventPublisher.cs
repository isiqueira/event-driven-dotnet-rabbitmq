using Shared.Events;

namespace Order.Worker.Services;

public interface IOrderWorkflowEventPublisher
{
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
