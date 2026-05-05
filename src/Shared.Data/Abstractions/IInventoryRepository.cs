using Shared.Models.Inventory;

namespace Shared.Data.Abstractions;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> GetBySkusAsync(
        IReadOnlyCollection<string> skus,
        CancellationToken cancellationToken);

    Task<InventoryItem?> GetReservableItemAsync(
        string sku,
        int quantity,
        CancellationToken cancellationToken);

    Task<InventoryItem?> GetItemAtLocationAsync(
        string sku,
        string warehouseId,
        string locationId,
        CancellationToken cancellationToken);

    Task<InventoryReservation?> GetReservationByIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken);

    Task<InventoryReservation?> GetReservationByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<InventoryReservation?> GetReservationForOrderAsync(
        Guid reservationId,
        Guid orderId,
        CancellationToken cancellationToken);

    Task AddReservationAsync(InventoryReservation reservation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
