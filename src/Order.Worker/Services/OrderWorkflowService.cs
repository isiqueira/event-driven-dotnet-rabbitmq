using Shared.Data.Abstractions;
using Shared.Events;
using Shared.Models.Orders;
using OrderEntity = Shared.Models.Orders.Order;

namespace Order.Worker.Services;

public sealed class OrderWorkflowService(
    IOrderRepository orderRepository,
    OrderProcessingService orderProcessingService,
    ILogger<OrderWorkflowService> logger)
{
    public async Task<OrderProcessedEvent> ProcessInventoryReservedAsync(
        InventoryReservedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(integrationEvent.OrderId, cancellationToken);

        if (order.Status == OrderStatus.Fulfilled)
        {
            logger.LogInformation(
                "Order {OrderId} is already fulfilled; re-emitting deterministic {EventType}",
                order.Id,
                OrderProcessedEvent.Name);

            return CreateOrderProcessedEvent(integrationEvent);
        }

        if (order.Status != OrderStatus.Processed)
        {
            order.MarkInventoryReserved(DateTimeOffset.UtcNow);
            order.MarkAsProcessing(DateTimeOffset.UtcNow);
            await orderProcessingService.ProcessAsync(order, cancellationToken);
            order.MarkAsProcessed(DateTimeOffset.UtcNow);
            await orderRepository.SaveChangesAsync(cancellationToken);
        }

        return CreateOrderProcessedEvent(integrationEvent);
    }

    public async Task<OrderFulfilledEvent> FulfillAsync(
        InventoryDeductedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(integrationEvent.OrderId, cancellationToken);

        if (order.Status != OrderStatus.Fulfilled)
        {
            order.MarkAsFulfilled(DateTimeOffset.UtcNow);
            await orderRepository.SaveChangesAsync(cancellationToken);
        }

        return new OrderFulfilledEvent(
            DeterministicEventId.Create($"{OrderFulfilledEvent.Name}:{integrationEvent.OrderId}:{integrationEvent.ReservationId}"),
            OrderFulfilledEvent.Name,
            DateTimeOffset.UtcNow,
            integrationEvent.CorrelationId,
            integrationEvent.OrderId,
            integrationEvent.ReservationId);
    }

    public async Task MarkInventoryReservationFailedAsync(
        InventoryReservationFailedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(integrationEvent.OrderId, cancellationToken);
        if (order.Status != OrderStatus.Fulfilled)
        {
            order.MarkAsFailed(DateTimeOffset.UtcNow);
            await orderRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<OrderEntity> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{orderId}' was not found.");
    }

    private static OrderProcessedEvent CreateOrderProcessedEvent(InventoryReservedEvent sourceEvent)
    {
        return new OrderProcessedEvent(
            DeterministicEventId.Create($"{OrderProcessedEvent.Name}:{sourceEvent.OrderId}:{sourceEvent.ReservationId}"),
            OrderProcessedEvent.Name,
            DateTimeOffset.UtcNow,
            sourceEvent.CorrelationId,
            sourceEvent.OrderId,
            sourceEvent.ReservationId,
            sourceEvent.Items);
    }
}
