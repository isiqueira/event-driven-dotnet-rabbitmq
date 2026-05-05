namespace Shared.Models.Orders;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    public static OrderItem Create(Guid orderId, string sku, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");
        }

        return Restore(Guid.NewGuid(), orderId, sku, quantity, unitPrice);
    }

    public static OrderItem Restore(Guid id, Guid orderId, string sku, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");
        }

        return new OrderItem
        {
            Id = id,
            OrderId = orderId,
            Sku = sku.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = quantity * unitPrice
        };
    }
}
