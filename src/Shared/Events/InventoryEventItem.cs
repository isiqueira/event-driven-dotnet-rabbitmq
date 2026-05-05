namespace Shared.Events;

public sealed record InventoryEventItem(
    string Sku,
    int Quantity,
    string WarehouseId,
    string LocationId);
