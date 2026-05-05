using Shared.Events;

namespace Order.Worker.Services;

public sealed class OrderProcessingService(ILogger<OrderProcessingService> logger)
{
    public async Task ProcessAsync(OrderCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing order {OrderId} for customer {CustomerId} with correlation ID {CorrelationId}",
            integrationEvent.OrderId,
            integrationEvent.CustomerId,
            integrationEvent.CorrelationId);

        if (string.Equals(integrationEvent.CustomerId, "fail-processing", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Simulated order processing failure.");
        }

        await Task.Delay(500, cancellationToken);
    }
}
