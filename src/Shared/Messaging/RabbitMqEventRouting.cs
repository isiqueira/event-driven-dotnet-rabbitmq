using Shared.Events;

namespace Shared.Messaging;

public static class RabbitMqEventRouting
{
    public static RabbitMqEventEndpoint GetEndpoint(
        RabbitMqOptions options,
        string eventType)
    {
        return eventType switch
        {
            OrderCreatedEvent.Name => new RabbitMqEventEndpoint(
                OrderCreatedEvent.Name,
                options.OrderCreatedQueue,
                options.OrderCreatedRetryQueue,
                options.OrderCreatedDeadLetterQueue,
                options.OrderCreatedRoutingKey,
                options.OrderCreatedRetryRoutingKey,
                options.OrderCreatedDeadLetterRoutingKey),

            InventoryReservedEvent.Name => new RabbitMqEventEndpoint(
                InventoryReservedEvent.Name,
                options.InventoryReservedQueue,
                options.InventoryReservedRetryQueue,
                options.InventoryReservedDeadLetterQueue,
                options.InventoryReservedRoutingKey,
                options.InventoryReservedRetryRoutingKey,
                options.InventoryReservedDeadLetterRoutingKey),

            InventoryReservationFailedEvent.Name => new RabbitMqEventEndpoint(
                InventoryReservationFailedEvent.Name,
                options.InventoryReservationFailedQueue,
                options.InventoryReservationFailedRetryQueue,
                options.InventoryReservationFailedDeadLetterQueue,
                options.InventoryReservationFailedRoutingKey,
                options.InventoryReservationFailedRetryRoutingKey,
                options.InventoryReservationFailedDeadLetterRoutingKey),

            OrderProcessedEvent.Name => new RabbitMqEventEndpoint(
                OrderProcessedEvent.Name,
                options.OrderProcessedQueue,
                options.OrderProcessedRetryQueue,
                options.OrderProcessedDeadLetterQueue,
                options.OrderProcessedRoutingKey,
                options.OrderProcessedRetryRoutingKey,
                options.OrderProcessedDeadLetterRoutingKey),

            InventoryDeductedEvent.Name => new RabbitMqEventEndpoint(
                InventoryDeductedEvent.Name,
                options.InventoryDeductedQueue,
                options.InventoryDeductedRetryQueue,
                options.InventoryDeductedDeadLetterQueue,
                options.InventoryDeductedRoutingKey,
                options.InventoryDeductedRetryRoutingKey,
                options.InventoryDeductedDeadLetterRoutingKey),

            OrderFulfilledEvent.Name => new RabbitMqEventEndpoint(
                OrderFulfilledEvent.Name,
                options.OrderFulfilledQueue,
                options.OrderFulfilledRetryQueue,
                options.OrderFulfilledDeadLetterQueue,
                options.OrderFulfilledRoutingKey,
                options.OrderFulfilledRetryRoutingKey,
                options.OrderFulfilledDeadLetterRoutingKey),

            _ => throw new InvalidOperationException($"No RabbitMQ endpoint is configured for event type '{eventType}'.")
        };
    }

    public static IReadOnlyCollection<RabbitMqEventEndpoint> GetAllEndpoints(RabbitMqOptions options)
    {
        return
        [
            GetEndpoint(options, OrderCreatedEvent.Name),
            GetEndpoint(options, InventoryReservedEvent.Name),
            GetEndpoint(options, InventoryReservationFailedEvent.Name),
            GetEndpoint(options, OrderProcessedEvent.Name),
            GetEndpoint(options, InventoryDeductedEvent.Name),
            GetEndpoint(options, OrderFulfilledEvent.Name)
        ];
    }
}
