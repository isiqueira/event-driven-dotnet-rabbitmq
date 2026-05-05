namespace Order.Worker.Models;

public enum OrderWorkflowStatus
{
    Created,
    PendingInventoryReservation,
    InventoryReserved,
    Processing,
    Processed,
    Fulfilled,
    Failed
}
