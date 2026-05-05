namespace Order.Worker.Models;

public sealed class OrderWorkflowOrderItem
{
    private OrderWorkflowOrderItem()
    {
    }

    public OrderWorkflowOrderItem(
        Guid id,
        Guid orderId,
        string sku,
        int quantity,
        decimal unitPrice)
    {
        Id = id;
        OrderId = orderId;
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
}
