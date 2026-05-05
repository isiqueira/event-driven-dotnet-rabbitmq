using Shared.Events;

namespace Inventory.Worker.Services;

public sealed class OrderProcessedMessageHandler(
    ProcessedMessageStore processedMessageStore,
    InventoryReservationService inventoryReservationService,
    IInventoryEventPublisher eventPublisher,
    ILogger<OrderProcessedMessageHandler> logger)
{
    public const string ConsumerName = "Inventory.Worker.OrderProcessed";

    public async Task HandleAsync(OrderProcessedEvent integrationEvent, CancellationToken cancellationToken = default)
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

        var inventoryDeducted = await inventoryReservationService.DeductAsync(integrationEvent, cancellationToken);
        await eventPublisher.PublishAsync(inventoryDeducted, cancellationToken);
        await processedMessageStore.MarkProcessedAsync(integrationEvent, ConsumerName, cancellationToken);
    }
}
