namespace Shared.Models.Inventory;

public sealed class InventoryItem
{
    private InventoryItem()
    {
    }

    public InventoryItem(
        string sku,
        string warehouseId,
        string locationId,
        int onHandQuantity,
        int availableQuantity,
        int reservedQuantity = 0)
    {
        if (onHandQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(onHandQuantity), "On-hand quantity cannot be negative.");
        }

        if (availableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity), "Available quantity cannot be negative.");
        }

        if (reservedQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reservedQuantity), "Reserved quantity cannot be negative.");
        }

        Id = Guid.NewGuid();
        Sku = sku.Trim();
        WarehouseId = warehouseId.Trim();
        LocationId = locationId.Trim();
        OnHandQuantity = onHandQuantity;
        AvailableQuantity = availableQuantity;
        ReservedQuantity = reservedQuantity;
    }

    public Guid Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string WarehouseId { get; private set; } = string.Empty;
    public string LocationId { get; private set; } = string.Empty;
    public int OnHandQuantity { get; private set; }
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }

    public bool CanReserve(int quantity)
    {
        return quantity > 0 && AvailableQuantity >= quantity;
    }

    public void Reserve(int quantity)
    {
        if (!CanReserve(quantity))
        {
            throw new InvalidOperationException($"Insufficient inventory for SKU '{Sku}'.");
        }

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
    }

    public void DeductReserved(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (ReservedQuantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient reserved inventory for SKU '{Sku}'.");
        }

        ReservedQuantity -= quantity;
        OnHandQuantity -= quantity;
    }
}
