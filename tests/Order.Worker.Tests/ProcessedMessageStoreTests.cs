using Microsoft.EntityFrameworkCore;
using Order.Worker.Data;
using Order.Worker.Services;
using Shared.Events;

namespace Order.Worker.Tests;

public sealed class ProcessedMessageStoreTests
{
    [Fact]
    public async Task HasProcessedAsync_returns_true_after_message_is_marked_processed()
    {
        await using var dbContext = CreateDbContext();
        var store = new ProcessedMessageStore(new EfProcessedMessageRepository(dbContext));
        var integrationEvent = new OrderCreatedEvent(
            Guid.NewGuid(),
            OrderCreatedEvent.Name,
            DateTimeOffset.UtcNow,
            "correlation-001",
            Guid.NewGuid(),
            "customer-001",
            100m,
            [new OrderCreatedItem("SKU-001", 1)]);
        const string consumerName = "test-consumer";

        var before = await store.HasProcessedAsync(
            integrationEvent.EventId,
            integrationEvent.EventType,
            consumerName);

        await store.MarkProcessedAsync(integrationEvent, consumerName);

        var after = await store.HasProcessedAsync(
            integrationEvent.EventId,
            integrationEvent.EventType,
            consumerName);

        Assert.False(before);
        Assert.True(after);
    }

    private static OrderWorkerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderWorkerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new OrderWorkerDbContext(options);
    }
}
