using Shared.Events;

namespace Order.Worker.Services;

public sealed class InventoryReservationFailedMessageHandler(
    ProcessedMessageStore processedMessageStore,
    OrderWorkflowService orderWorkflowService,
    ILogger<InventoryReservationFailedMessageHandler> logger)
{
    public const string ConsumerName = "Order.Worker.InventoryReservationFailed";

    public async Task HandleAsync(InventoryReservationFailedEvent integrationEvent, CancellationToken cancellationToken = default)
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

        await orderWorkflowService.MarkInventoryReservationFailedAsync(integrationEvent, cancellationToken);
        await processedMessageStore.MarkProcessedAsync(integrationEvent, ConsumerName, cancellationToken);
    }
}
