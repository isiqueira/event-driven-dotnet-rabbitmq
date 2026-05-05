using Shared.Events;

namespace Order.Api.Messaging;

public interface IOrderEventPublisher
{
    Task PublishAsync(OrderCreatedEvent integrationEvent, CancellationToken cancellationToken = default);
}
