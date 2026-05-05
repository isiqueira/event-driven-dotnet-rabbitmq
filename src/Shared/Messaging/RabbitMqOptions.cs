namespace Shared.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "app";
    public string Password { get; init; } = "app";
    public string ExchangeName { get; init; } = "orders.exchange";
    public string OrderCreatedQueue { get; init; } = "orders.created.queue";
    public string OrderCreatedRetryQueue { get; init; } = "orders.created.retry.queue";
    public string OrderCreatedDeadLetterQueue { get; init; } = "orders.created.dlq";
    public string OrderCreatedRoutingKey { get; init; } = "orders.created";
    public string OrderCreatedRetryRoutingKey { get; init; } = "orders.created.retry";
    public string OrderCreatedDeadLetterRoutingKey { get; init; } = "orders.created.dead-letter";
    public string InventoryReservedQueue { get; init; } = "inventory.reserved.queue";
    public string InventoryReservedRetryQueue { get; init; } = "inventory.reserved.retry.queue";
    public string InventoryReservedDeadLetterQueue { get; init; } = "inventory.reserved.dlq";
    public string InventoryReservedRoutingKey { get; init; } = "inventory.reserved";
    public string InventoryReservedRetryRoutingKey { get; init; } = "inventory.reserved.retry";
    public string InventoryReservedDeadLetterRoutingKey { get; init; } = "inventory.reserved.dead-letter";
    public string InventoryReservationFailedQueue { get; init; } = "inventory.reservation-failed.queue";
    public string InventoryReservationFailedRetryQueue { get; init; } = "inventory.reservation-failed.retry.queue";
    public string InventoryReservationFailedDeadLetterQueue { get; init; } = "inventory.reservation-failed.dlq";
    public string InventoryReservationFailedRoutingKey { get; init; } = "inventory.reservation-failed";
    public string InventoryReservationFailedRetryRoutingKey { get; init; } = "inventory.reservation-failed.retry";
    public string InventoryReservationFailedDeadLetterRoutingKey { get; init; } = "inventory.reservation-failed.dead-letter";
    public string OrderProcessedQueue { get; init; } = "orders.processed.queue";
    public string OrderProcessedRetryQueue { get; init; } = "orders.processed.retry.queue";
    public string OrderProcessedDeadLetterQueue { get; init; } = "orders.processed.dlq";
    public string OrderProcessedRoutingKey { get; init; } = "orders.processed";
    public string OrderProcessedRetryRoutingKey { get; init; } = "orders.processed.retry";
    public string OrderProcessedDeadLetterRoutingKey { get; init; } = "orders.processed.dead-letter";
    public string InventoryDeductedQueue { get; init; } = "inventory.deducted.queue";
    public string InventoryDeductedRetryQueue { get; init; } = "inventory.deducted.retry.queue";
    public string InventoryDeductedDeadLetterQueue { get; init; } = "inventory.deducted.dlq";
    public string InventoryDeductedRoutingKey { get; init; } = "inventory.deducted";
    public string InventoryDeductedRetryRoutingKey { get; init; } = "inventory.deducted.retry";
    public string InventoryDeductedDeadLetterRoutingKey { get; init; } = "inventory.deducted.dead-letter";
    public string OrderFulfilledQueue { get; init; } = "orders.fulfilled.queue";
    public string OrderFulfilledRetryQueue { get; init; } = "orders.fulfilled.retry.queue";
    public string OrderFulfilledDeadLetterQueue { get; init; } = "orders.fulfilled.dlq";
    public string OrderFulfilledRoutingKey { get; init; } = "orders.fulfilled";
    public string OrderFulfilledRetryRoutingKey { get; init; } = "orders.fulfilled.retry";
    public string OrderFulfilledDeadLetterRoutingKey { get; init; } = "orders.fulfilled.dead-letter";
    public int MaxRetryCount { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 5000;
}
