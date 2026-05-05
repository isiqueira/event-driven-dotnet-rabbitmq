using OrderEntity = Shared.Models.Orders.Order;

namespace Order.Worker.Services;

public sealed class OrderProcessingService(ILogger<OrderProcessingService> logger)
{
    public Task ProcessAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing order {OrderId} for customer {CustomerId}",
            order.Id,
            order.CustomerId);

        if (string.Equals(order.CustomerId, "fail-processing", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Simulated order processing failure.");
        }

        return Task.CompletedTask;
    }
}
