using Shared.Events;

namespace Order.Worker.Services;

public sealed class OrderCreatedMessageHandler(
    ProcessedMessageStore processedMessageStore,
    OrderProcessingService orderProcessingService,
    ILogger<OrderCreatedMessageHandler> logger)
{
    public const string ConsumerName = "Order.Worker";

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
                "Skipping duplicate {EventType} event {EventId} for consumer {Consumer} with correlation ID {CorrelationId}",
                integrationEvent.EventType,
                integrationEvent.EventId,
                ConsumerName,
                integrationEvent.CorrelationId);
            return;
        }

        await orderProcessingService.ProcessAsync(integrationEvent, cancellationToken);

        await processedMessageStore.MarkProcessedAsync(
            integrationEvent,
            ConsumerName,
            cancellationToken);

        logger.LogInformation(
            "Successfully processed {EventType} event {EventId} for consumer {Consumer} with correlation ID {CorrelationId}",
            integrationEvent.EventType,
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.CorrelationId);
    }
}
