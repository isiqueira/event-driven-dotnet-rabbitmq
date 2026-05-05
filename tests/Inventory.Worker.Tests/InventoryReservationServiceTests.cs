using Inventory.Worker.Data;
using Inventory.Worker.Models;
using Inventory.Worker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Events;

namespace Inventory.Worker.Tests;

public sealed class InventoryReservationServiceTests
{
    [Fact]
    public async Task ReserveAsync_reserves_available_inventory_and_returns_inventory_reserved()
    {
        await using var dbContext = CreateDbContext();
        dbContext.InventoryItems.Add(new InventoryItem("SKU-001", "WH-01", "A1-01", 10, 10));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var orderCreated = CreateOrderCreated(quantity: 2);

        var result = await service.ReserveAsync(orderCreated);

        var inventoryReserved = Assert.IsType<InventoryReservedEvent>(result);
        var inventoryItem = await dbContext.InventoryItems.SingleAsync();

        Assert.Equal(orderCreated.OrderId, inventoryReserved.OrderId);
        Assert.NotEqual(Guid.Empty, inventoryReserved.ReservationId);
        Assert.Equal(8, inventoryItem.AvailableQuantity);
        Assert.Equal(2, inventoryItem.ReservedQuantity);
        Assert.Equal(10, inventoryItem.OnHandQuantity);
    }

    [Fact]
    public async Task ReserveAsync_returns_inventory_reservation_failed_when_inventory_is_insufficient()
    {
        await using var dbContext = CreateDbContext();
        dbContext.InventoryItems.Add(new InventoryItem("SKU-001", "WH-01", "A1-01", 1, 1));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.ReserveAsync(CreateOrderCreated(quantity: 2));

        var failed = Assert.IsType<InventoryReservationFailedEvent>(result);
        var inventoryItem = await dbContext.InventoryItems.SingleAsync();

        Assert.Contains("Insufficient inventory", failed.Reason);
        Assert.Equal(1, inventoryItem.AvailableQuantity);
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(1, inventoryItem.OnHandQuantity);
    }

    [Fact]
    public async Task DeductAsync_deducts_from_reserved_inventory_only()
    {
        await using var dbContext = CreateDbContext();
        dbContext.InventoryItems.Add(new InventoryItem("SKU-001", "WH-01", "A1-01", 10, 10));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var orderCreated = CreateOrderCreated(quantity: 2);
        var inventoryReserved = Assert.IsType<InventoryReservedEvent>(await service.ReserveAsync(orderCreated));

        var inventoryDeducted = await service.DeductAsync(new OrderProcessedEvent(
            Guid.NewGuid(),
            OrderProcessedEvent.Name,
            DateTimeOffset.UtcNow,
            orderCreated.CorrelationId,
            orderCreated.OrderId,
            inventoryReserved.ReservationId,
            inventoryReserved.Items));

        var inventoryItem = await dbContext.InventoryItems.SingleAsync();

        Assert.Equal(inventoryReserved.ReservationId, inventoryDeducted.ReservationId);
        Assert.Equal(8, inventoryItem.AvailableQuantity);
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(8, inventoryItem.OnHandQuantity);
    }

    [Fact]
    public async Task ReserveAsync_is_idempotent_for_the_same_order()
    {
        await using var dbContext = CreateDbContext();
        dbContext.InventoryItems.Add(new InventoryItem("SKU-001", "WH-01", "A1-01", 10, 10));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var orderCreated = CreateOrderCreated(quantity: 2);

        var first = Assert.IsType<InventoryReservedEvent>(await service.ReserveAsync(orderCreated));
        var second = Assert.IsType<InventoryReservedEvent>(await service.ReserveAsync(orderCreated));
        var inventoryItem = await dbContext.InventoryItems.SingleAsync();

        Assert.Equal(first.ReservationId, second.ReservationId);
        Assert.Equal(first.EventId, second.EventId);
        Assert.Equal(8, inventoryItem.AvailableQuantity);
        Assert.Equal(2, inventoryItem.ReservedQuantity);
    }

    [Fact]
    public async Task OrderCreated_handler_is_idempotent_for_the_same_event()
    {
        await using var dbContext = CreateDbContext();
        dbContext.InventoryItems.Add(new InventoryItem("SKU-001", "WH-01", "A1-01", 10, 10));
        await dbContext.SaveChangesAsync();

        var publisher = new RecordingInventoryEventPublisher();
        var handler = new OrderCreatedMessageHandler(
            new ProcessedMessageStore(dbContext),
            CreateService(dbContext),
            publisher,
            NullLogger<OrderCreatedMessageHandler>.Instance);
        var orderCreated = CreateOrderCreated(quantity: 2);

        await handler.HandleAsync(orderCreated);
        await handler.HandleAsync(orderCreated);

        var inventoryItem = await dbContext.InventoryItems.SingleAsync();

        Assert.Single(publisher.Events);
        Assert.Equal(8, inventoryItem.AvailableQuantity);
        Assert.Equal(2, inventoryItem.ReservedQuantity);
    }

    private static InventoryReservationService CreateService(InventoryDbContext dbContext)
    {
        return new InventoryReservationService(
            dbContext,
            NullLogger<InventoryReservationService>.Instance);
    }

    private static OrderCreatedEvent CreateOrderCreated(int quantity)
    {
        return new OrderCreatedEvent(
            Guid.NewGuid(),
            OrderCreatedEvent.Name,
            DateTimeOffset.UtcNow,
            "correlation-001",
            Guid.NewGuid(),
            "customer-001",
            100m * quantity,
            [new OrderCreatedItem("SKU-001", quantity)]);
    }

    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new InventoryDbContext(options);
    }

    private sealed class RecordingInventoryEventPublisher : IInventoryEventPublisher
    {
        public List<IntegrationEvent> Events { get; } = [];

        public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
