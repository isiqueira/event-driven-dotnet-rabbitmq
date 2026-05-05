using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Order.Worker.Data;
using Order.Worker.Services;
using Shared.Events;
using Shared.Models.Orders;
using OrderEntity = Shared.Models.Orders.Order;

namespace Order.Worker.Tests;

public sealed class OrderWorkflowHandlersTests
{
    [Fact]
    public async Task InventoryReserved_handler_processes_order_and_publishes_order_processed()
    {
        var orderId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        await using var workerDbContext = CreateWorkerDbContext();
        await using var orderDbContext = CreateOrderWorkflowDbContext();
        orderDbContext.Orders.Add(CreateOrder(orderId, OrderStatus.PendingInventoryReservation));
        await orderDbContext.SaveChangesAsync();

        var publisher = new RecordingOrderWorkflowEventPublisher();
        var handler = new InventoryReservedMessageHandler(
            new ProcessedMessageStore(new EfProcessedMessageRepository(workerDbContext)),
            new OrderWorkflowService(
                new EfOrderRepository(orderDbContext),
                new OrderProcessingService(NullLogger<OrderProcessingService>.Instance),
                NullLogger<OrderWorkflowService>.Instance),
            publisher,
            NullLogger<InventoryReservedMessageHandler>.Instance);

        await handler.HandleAsync(new InventoryReservedEvent(
            Guid.NewGuid(),
            InventoryReservedEvent.Name,
            DateTimeOffset.UtcNow,
            "correlation-001",
            orderId,
            reservationId,
            [new InventoryEventItem("SKU-001", 2, "WH-01", "A1-01")]));

        var order = await orderDbContext.Orders.SingleAsync(existingOrder => existingOrder.Id == orderId);
        var publishedEvent = Assert.IsType<OrderProcessedEvent>(Assert.Single(publisher.Events));

        Assert.Equal(OrderStatus.Processed, order.Status);
        Assert.Equal(orderId, publishedEvent.OrderId);
        Assert.Equal(reservationId, publishedEvent.ReservationId);
    }

    [Fact]
    public async Task InventoryDeducted_handler_marks_order_as_fulfilled()
    {
        var orderId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        await using var workerDbContext = CreateWorkerDbContext();
        await using var orderDbContext = CreateOrderWorkflowDbContext();
        orderDbContext.Orders.Add(CreateOrder(orderId, OrderStatus.Processed));
        await orderDbContext.SaveChangesAsync();

        var publisher = new RecordingOrderWorkflowEventPublisher();
        var handler = new InventoryDeductedMessageHandler(
            new ProcessedMessageStore(new EfProcessedMessageRepository(workerDbContext)),
            new OrderWorkflowService(
                new EfOrderRepository(orderDbContext),
                new OrderProcessingService(NullLogger<OrderProcessingService>.Instance),
                NullLogger<OrderWorkflowService>.Instance),
            publisher,
            NullLogger<InventoryDeductedMessageHandler>.Instance);

        await handler.HandleAsync(new InventoryDeductedEvent(
            Guid.NewGuid(),
            InventoryDeductedEvent.Name,
            DateTimeOffset.UtcNow,
            "correlation-001",
            orderId,
            reservationId,
            [new InventoryEventItem("SKU-001", 2, "WH-01", "A1-01")]));

        var order = await orderDbContext.Orders.SingleAsync(existingOrder => existingOrder.Id == orderId);
        var publishedEvent = Assert.IsType<OrderFulfilledEvent>(Assert.Single(publisher.Events));

        Assert.Equal(OrderStatus.Fulfilled, order.Status);
        Assert.Equal(orderId, publishedEvent.OrderId);
        Assert.Equal(reservationId, publishedEvent.ReservationId);
    }

    private static OrderEntity CreateOrder(Guid orderId, OrderStatus status)
    {
        return OrderEntity.Restore(
            orderId,
            "customer-001",
            200m,
            status,
            DateTimeOffset.UtcNow,
            [OrderItem.Restore(Guid.NewGuid(), orderId, "SKU-001", 2, 100m)]);
    }

    private static OrderWorkerDbContext CreateWorkerDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderWorkerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new OrderWorkerDbContext(options);
    }

    private static OrderWorkflowDbContext CreateOrderWorkflowDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderWorkflowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new OrderWorkflowDbContext(options);
    }

    private sealed class RecordingOrderWorkflowEventPublisher : IOrderWorkflowEventPublisher
    {
        public List<IntegrationEvent> Events { get; } = [];

        public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
