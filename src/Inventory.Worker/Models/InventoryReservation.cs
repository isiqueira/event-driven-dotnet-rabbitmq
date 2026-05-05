namespace Inventory.Worker.Models;

public sealed class InventoryReservation
{
    private InventoryReservation()
    {
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public InventoryReservationStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeductedAt { get; private set; }
    public List<InventoryReservationItem> Items { get; private set; } = [];

    public static InventoryReservation CreateReserved(
        Guid orderId,
        IEnumerable<InventoryReservationItem> items,
        DateTimeOffset createdAt)
    {
        var reservationId = Guid.NewGuid();
        var reservation = new InventoryReservation
        {
            Id = reservationId,
            OrderId = orderId,
            Status = InventoryReservationStatus.Reserved,
            CreatedAt = createdAt
        };

        foreach (var item in items)
        {
            item.AssignReservation(reservationId);
            reservation.Items.Add(item);
        }

        if (reservation.Items.Count == 0)
        {
            throw new InvalidOperationException("A reservation must contain at least one item.");
        }

        return reservation;
    }

    public static InventoryReservation CreateFailed(
        Guid orderId,
        string reason,
        DateTimeOffset createdAt)
    {
        return new InventoryReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = InventoryReservationStatus.Failed,
            FailureReason = reason,
            CreatedAt = createdAt
        };
    }

    public void MarkDeducted(DateTimeOffset deductedAt)
    {
        if (Status == InventoryReservationStatus.Deducted)
        {
            return;
        }

        if (Status != InventoryReservationStatus.Reserved)
        {
            throw new InvalidOperationException("Only reserved inventory can be deducted.");
        }

        Status = InventoryReservationStatus.Deducted;
        DeductedAt = deductedAt;
        UpdatedAt = deductedAt;
    }
}
