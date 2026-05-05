using Shared.Events;

namespace Inventory.Worker.Services;

public sealed class OrderCreatedMessageHandler(
    ProcessedMessageStore processedMessageStore,
    InventoryReservationService inventoryReservationService,
    IInventoryEventPublisher eventPublisher,
    ILogger<OrderCreatedMessageHandler> logger)
{
    public const string ConsumerName = "Inventory.Worker.OrderCreated";

    public async Task HandleAsync(OrderCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await processedMessageStore.HasProcessedAsync(
            integrationEvent.EventId,
            integrationEvent.EventType,
            ConsumerName,
            cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation(
                "Skipping duplicate {EventType} event {EventId} for consumer {Consumer}",
                integrationEvent.EventType,
                integrationEvent.EventId,
                ConsumerName);
            return;
        }

        var resultEvent = await inventoryReservationService.ReserveAsync(integrationEvent, cancellationToken);
        await eventPublisher.PublishAsync(resultEvent, cancellationToken);
        await processedMessageStore.MarkProcessedAsync(integrationEvent, ConsumerName, cancellationToken);
    }
}
