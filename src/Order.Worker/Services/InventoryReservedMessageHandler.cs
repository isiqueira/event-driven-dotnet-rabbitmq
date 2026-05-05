using Shared.Events;

namespace Order.Worker.Services;

public sealed class InventoryReservedMessageHandler(
    ProcessedMessageStore processedMessageStore,
    OrderWorkflowService orderWorkflowService,
    IOrderWorkflowEventPublisher eventPublisher,
    ILogger<InventoryReservedMessageHandler> logger)
{
    public const string ConsumerName = "Order.Worker.InventoryReserved";

    public async Task HandleAsync(InventoryReservedEvent integrationEvent, CancellationToken cancellationToken = default)
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

        var orderProcessed = await orderWorkflowService.ProcessInventoryReservedAsync(integrationEvent, cancellationToken);
        await eventPublisher.PublishAsync(orderProcessed, cancellationToken);
        await processedMessageStore.MarkProcessedAsync(integrationEvent, ConsumerName, cancellationToken);
    }
}
