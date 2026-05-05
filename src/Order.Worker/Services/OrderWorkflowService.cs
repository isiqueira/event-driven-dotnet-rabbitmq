using Microsoft.EntityFrameworkCore;
using Order.Worker.Data;
using Order.Worker.Models;
using Shared.Events;

namespace Order.Worker.Services;

public sealed class OrderWorkflowService(
    OrderWorkflowDbContext dbContext,
    OrderProcessingService orderProcessingService,
    ILogger<OrderWorkflowService> logger)
{
    public async Task<OrderProcessedEvent> ProcessInventoryReservedAsync(
        InventoryReservedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(integrationEvent.OrderId, cancellationToken);

        if (order.Status == OrderWorkflowStatus.Fulfilled)
        {
            logger.LogInformation(
                "Order {OrderId} is already fulfilled; re-emitting deterministic {EventType}",
                order.Id,
                OrderProcessedEvent.Name);

            return CreateOrderProcessedEvent(integrationEvent);
        }

        if (order.Status != OrderWorkflowStatus.Processed)
        {
            order.MarkInventoryReserved(DateTimeOffset.UtcNow);
            order.MarkProcessing(DateTimeOffset.UtcNow);
            await orderProcessingService.ProcessAsync(order, cancellationToken);
            order.MarkProcessed(DateTimeOffset.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return CreateOrderProcessedEvent(integrationEvent);
    }

    public async Task<OrderFulfilledEvent> FulfillAsync(
        InventoryDeductedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(integrationEvent.OrderId, cancellationToken);

        if (order.Status != OrderWorkflowStatus.Fulfilled)
        {
            order.MarkFulfilled(DateTimeOffset.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
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
        order.MarkFailed(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<OrderWorkflowOrder> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken)
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
