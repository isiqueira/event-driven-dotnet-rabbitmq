using Microsoft.EntityFrameworkCore;
using Shared.Data.Abstractions;
using Shared.Models.Inventory;

namespace Inventory.Worker.Data;

public sealed class EfInventoryRepository(InventoryDbContext dbContext) : IInventoryRepository
{
    public async Task<IReadOnlyList<InventoryItem>> GetBySkusAsync(
        IReadOnlyCollection<string> skus,
        CancellationToken cancellationToken)
    {
        return await dbContext.InventoryItems
            .Where(item => skus.Contains(item.Sku))
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItem?> GetReservableItemAsync(
        string sku,
        int quantity,
        CancellationToken cancellationToken)
    {
        return await dbContext.InventoryItems
            .Where(item => item.Sku == sku && item.AvailableQuantity >= quantity)
            .OrderBy(item => item.WarehouseId)
            .ThenBy(item => item.LocationId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<InventoryItem?> GetItemAtLocationAsync(
        string sku,
        string warehouseId,
        string locationId,
        CancellationToken cancellationToken)
    {
        return dbContext.InventoryItems.FirstOrDefaultAsync(
            item => item.Sku == sku
                    && item.WarehouseId == warehouseId
                    && item.LocationId == locationId,
            cancellationToken);
    }

    public async Task<InventoryReservation?> GetReservationByIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.InventoryReservations
            .Include(reservation => reservation.Items)
            .FirstOrDefaultAsync(reservation => reservation.Id == reservationId, cancellationToken);
    }

    public async Task<InventoryReservation?> GetReservationByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await dbContext.InventoryReservations
            .Include(reservation => reservation.Items)
            .FirstOrDefaultAsync(reservation => reservation.OrderId == orderId, cancellationToken);
    }

    public async Task<InventoryReservation?> GetReservationForOrderAsync(
        Guid reservationId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await dbContext.InventoryReservations
            .Include(reservation => reservation.Items)
            .FirstOrDefaultAsync(
                reservation => reservation.Id == reservationId && reservation.OrderId == orderId,
                cancellationToken);
    }

    public async Task AddReservationAsync(InventoryReservation reservation, CancellationToken cancellationToken)
    {
        await dbContext.InventoryReservations.AddAsync(reservation, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
