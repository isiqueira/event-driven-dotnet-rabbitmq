namespace Order.Api.Domain;

public sealed class Order
{
    private Order()
    {
    }

    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];

    public static Order Create(
        string customerId,
        IEnumerable<OrderItemInput> items,
        DateTimeOffset createdAt)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Trim(),
            Status = OrderStatus.Created,
            CreatedAt = createdAt
        };

        foreach (var item in items)
        {
            order.Items.Add(OrderItem.Create(order.Id, item.Sku, item.Quantity, item.UnitPrice));
        }

        if (order.Items.Count == 0)
        {
            throw new InvalidOperationException("An order must contain at least one item.");
        }

        order.TotalAmount = order.Items.Sum(item => item.TotalPrice);
        return order;
    }

    public void MarkAsProcessing(DateTimeOffset updatedAt)
    {
        Status = OrderStatus.Processing;
        UpdatedAt = updatedAt;
    }

    public void MarkAsProcessed(DateTimeOffset updatedAt)
    {
        Status = OrderStatus.Processed;
        UpdatedAt = updatedAt;
    }

    public void MarkAsFailed(DateTimeOffset updatedAt)
    {
        Status = OrderStatus.Failed;
        UpdatedAt = updatedAt;
    }
}
