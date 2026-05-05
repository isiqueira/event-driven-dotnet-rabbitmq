using Inventory.Worker.Data;
using Inventory.Worker.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace Inventory.Worker.Services;

public sealed class InventoryReservationService(
    InventoryDbContext dbContext,
    ILogger<InventoryReservationService> logger)
{
    public async Task<IntegrationEvent> ReserveAsync(
        OrderCreatedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var existingReservation = await dbContext.InventoryReservations
            .Include(reservation => reservation.Items)
            .FirstOrDefaultAsync(
                reservation => reservation.OrderId == integrationEvent.OrderId,
                cancellationToken);

        if (existingReservation is not null)
        {
            logger.LogInformation(
                "Reusing existing inventory reservation {ReservationId} with status {Status} for order {OrderId}",
                existingReservation.Id,
                existingReservation.Status,
                integrationEvent.OrderId);

            return existingReservation.Status == InventoryReservationStatus.Failed
                ? CreateReservationFailedEvent(integrationEvent, existingReservation.FailureReason ?? "Inventory reservation failed.")
                : CreateInventoryReservedEvent(integrationEvent, existingReservation);
        }

        var requestedItems = integrationEvent.Items
            .GroupBy(item => item.Sku.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new OrderCreatedItem(group.Key, group.Sum(item => item.Quantity)))
            .ToArray();

        var reservableItems = new List<(InventoryItem InventoryItem, OrderCreatedItem RequestedItem)>();

        foreach (var requestedItem in requestedItems)
        {
            var inventoryItem = await dbContext.InventoryItems
                .Where(item => item.Sku == requestedItem.Sku && item.AvailableQuantity >= requestedItem.Quantity)
                .OrderBy(item => item.WarehouseId)
                .ThenBy(item => item.LocationId)
                .FirstOrDefaultAsync(cancellationToken);

            if (inventoryItem is null)
            {
                var reason = $"Insufficient inventory for SKU '{requestedItem.Sku}'.";
                var failedReservation = InventoryReservation.CreateFailed(
                    integrationEvent.OrderId,
                    reason,
                    DateTimeOffset.UtcNow);

                dbContext.InventoryReservations.Add(failedReservation);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogWarning(
                    "Inventory reservation failed for order {OrderId}: {Reason}",
                    integrationEvent.OrderId,
                    reason);

                return CreateReservationFailedEvent(integrationEvent, reason);
            }

            reservableItems.Add((inventoryItem, requestedItem));
        }

        var reservationItems = new List<InventoryReservationItem>();
        foreach (var (inventoryItem, requestedItem) in reservableItems)
        {
            inventoryItem.Reserve(requestedItem.Quantity);
            reservationItems.Add(new InventoryReservationItem(
                Guid.Empty,
                requestedItem.Sku,
                requestedItem.Quantity,
                inventoryItem.WarehouseId,
                inventoryItem.LocationId));
        }

        var reservation = InventoryReservation.CreateReserved(
            integrationEvent.OrderId,
            reservationItems,
            DateTimeOffset.UtcNow);

        dbContext.InventoryReservations.Add(reservation);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Reserved inventory {ReservationId} for order {OrderId} with {ItemCount} item(s)",
            reservation.Id,
            integrationEvent.OrderId,
            reservableItems.Count);

        return CreateInventoryReservedEvent(integrationEvent, reservation);
    }

    public async Task<InventoryDeductedEvent> DeductAsync(
        OrderProcessedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.InventoryReservations
            .Include(existingReservation => existingReservation.Items)
            .FirstOrDefaultAsync(
                existingReservation => existingReservation.Id == integrationEvent.ReservationId
                                       && existingReservation.OrderId == integrationEvent.OrderId,
                cancellationToken);

        if (reservation is null)
        {
            throw new InvalidOperationException(
                $"Inventory reservation '{integrationEvent.ReservationId}' was not found for order '{integrationEvent.OrderId}'.");
        }

        if (reservation.Status == InventoryReservationStatus.Deducted)
        {
            logger.LogInformation(
                "Skipping duplicate inventory deduction for reservation {ReservationId}",
                reservation.Id);

            return CreateInventoryDeductedEvent(integrationEvent, reservation);
        }

        if (reservation.Status != InventoryReservationStatus.Reserved)
        {
            throw new InvalidOperationException(
                $"Inventory reservation '{reservation.Id}' cannot be deducted from status '{reservation.Status}'.");
        }

        foreach (var reservationItem in reservation.Items)
        {
            var inventoryItem = await dbContext.InventoryItems.FirstOrDefaultAsync(
                item => item.Sku == reservationItem.Sku
                        && item.WarehouseId == reservationItem.WarehouseId
                        && item.LocationId == reservationItem.LocationId,
                cancellationToken);

            if (inventoryItem is null)
            {
                throw new InvalidOperationException(
                    $"Inventory item '{reservationItem.Sku}' at '{reservationItem.WarehouseId}/{reservationItem.LocationId}' was not found.");
            }

            inventoryItem.DeductReserved(reservationItem.Quantity);
        }

        reservation.MarkDeducted(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Deducted reserved inventory for reservation {ReservationId} and order {OrderId}",
            reservation.Id,
            reservation.OrderId);

        return CreateInventoryDeductedEvent(integrationEvent, reservation);
    }

    private static InventoryReservedEvent CreateInventoryReservedEvent(
        OrderCreatedEvent sourceEvent,
        InventoryReservation reservation)
    {
        return new InventoryReservedEvent(
            DeterministicEventId.Create($"{InventoryReservedEvent.Name}:{reservation.Id}"),
            InventoryReservedEvent.Name,
            DateTimeOffset.UtcNow,
            sourceEvent.CorrelationId,
            sourceEvent.OrderId,
            reservation.Id,
            reservation.Items
                .Select(item => new InventoryEventItem(
                    item.Sku,
                    item.Quantity,
                    item.WarehouseId,
                    item.LocationId))
                .ToArray());
    }

    private static InventoryReservationFailedEvent CreateReservationFailedEvent(
        OrderCreatedEvent sourceEvent,
        string reason)
    {
        return new InventoryReservationFailedEvent(
            DeterministicEventId.Create($"{InventoryReservationFailedEvent.Name}:{sourceEvent.OrderId}"),
            InventoryReservationFailedEvent.Name,
            DateTimeOffset.UtcNow,
            sourceEvent.CorrelationId,
            sourceEvent.OrderId,
            reason,
            sourceEvent.Items);
    }

    private static InventoryDeductedEvent CreateInventoryDeductedEvent(
        OrderProcessedEvent sourceEvent,
        InventoryReservation reservation)
    {
        return new InventoryDeductedEvent(
            DeterministicEventId.Create($"{InventoryDeductedEvent.Name}:{reservation.Id}"),
            InventoryDeductedEvent.Name,
            DateTimeOffset.UtcNow,
            sourceEvent.CorrelationId,
            sourceEvent.OrderId,
            reservation.Id,
            reservation.Items
                .Select(item => new InventoryEventItem(
                    item.Sku,
                    item.Quantity,
                    item.WarehouseId,
                    item.LocationId))
                .ToArray());
    }
}
