namespace Shared.Models.Orders;

public enum OrderStatus
{
    Created,
    PendingInventoryReservation,
    InventoryReserved,
    Processing,
    Processed,
    Fulfilled,
    Failed
}
