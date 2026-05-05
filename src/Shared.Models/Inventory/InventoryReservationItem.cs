namespace Shared.Models.Inventory;

public sealed class InventoryReservationItem
{
    private InventoryReservationItem()
    {
    }

    public InventoryReservationItem(
        Guid reservationId,
        string sku,
        int quantity,
        string warehouseId,
        string locationId)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        ReservationId = reservationId;
        Sku = sku.Trim();
        Quantity = quantity;
        WarehouseId = warehouseId.Trim();
        LocationId = locationId.Trim();
    }

    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string WarehouseId { get; private set; } = string.Empty;
    public string LocationId { get; private set; } = string.Empty;

    public void AssignReservation(Guid reservationId)
    {
        ReservationId = reservationId;
    }
}
