using Shared.Events;

namespace Order.Worker.Services;

public sealed class InventoryDeductedMessageHandler(
    ProcessedMessageStore processedMessageStore,
    OrderWorkflowService orderWorkflowService,
    IOrderWorkflowEventPublisher eventPublisher,
    ILogger<InventoryDeductedMessageHandler> logger)
{
    public const string ConsumerName = "Order.Worker.InventoryDeducted";

    public async Task HandleAsync(InventoryDeductedEvent integrationEvent, CancellationToken cancellationToken = default)
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

        var orderFulfilled = await orderWorkflowService.FulfillAsync(integrationEvent, cancellationToken);
        await eventPublisher.PublishAsync(orderFulfilled, cancellationToken);
        await processedMessageStore.MarkProcessedAsync(integrationEvent, ConsumerName, cancellationToken);
    }
}
