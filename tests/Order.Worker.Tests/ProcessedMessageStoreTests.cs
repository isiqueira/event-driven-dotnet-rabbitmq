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
        var store = new ProcessedMessageStore(dbContext);
        var integrationEvent = new OrderCreatedEvent(
            Guid.NewGuid(),
            OrderCreatedEvent.Name,
            DateTimeOffset.UtcNow,
            "correlation-001",
            Guid.NewGuid(),
            "customer-001",
            100m);

        var before = await store.HasProcessedAsync(
            integrationEvent.EventId,
            integrationEvent.EventType,
            OrderCreatedMessageHandler.ConsumerName);

        await store.MarkProcessedAsync(integrationEvent, OrderCreatedMessageHandler.ConsumerName);

        var after = await store.HasProcessedAsync(
            integrationEvent.EventId,
            integrationEvent.EventType,
            OrderCreatedMessageHandler.ConsumerName);

        Assert.False(before);
        Assert.True(after);
    }

    private static WorkerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new WorkerDbContext(options);
    }
}
