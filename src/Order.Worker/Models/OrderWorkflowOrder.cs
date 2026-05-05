namespace Order.Worker.Models;

public sealed class OrderWorkflowOrder
{
    private OrderWorkflowOrder()
    {
    }

    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderWorkflowStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public List<OrderWorkflowOrderItem> Items { get; private set; } = [];

    public static OrderWorkflowOrder Create(
        Guid id,
        string customerId,
        decimal totalAmount,
        OrderWorkflowStatus status,
        DateTimeOffset createdAt,
        IEnumerable<OrderWorkflowOrderItem> items)
    {
        var order = new OrderWorkflowOrder
        {
            Id = id,
            CustomerId = customerId,
            TotalAmount = totalAmount,
            Status = status,
            CreatedAt = createdAt
        };

        order.Items.AddRange(items);
        return order;
    }

    public void MarkInventoryReserved(DateTimeOffset updatedAt)
    {
        if (Status is OrderWorkflowStatus.Processed or OrderWorkflowStatus.Fulfilled)
        {
            return;
        }

        Status = OrderWorkflowStatus.InventoryReserved;
        UpdatedAt = updatedAt;
    }

    public void MarkProcessing(DateTimeOffset updatedAt)
    {
        if (Status is OrderWorkflowStatus.Processed or OrderWorkflowStatus.Fulfilled)
        {
            return;
        }

        Status = OrderWorkflowStatus.Processing;
        UpdatedAt = updatedAt;
    }

    public void MarkProcessed(DateTimeOffset updatedAt)
    {
        if (Status == OrderWorkflowStatus.Fulfilled)
        {
            return;
        }

        Status = OrderWorkflowStatus.Processed;
        UpdatedAt = updatedAt;
    }

    public void MarkFulfilled(DateTimeOffset updatedAt)
    {
        Status = OrderWorkflowStatus.Fulfilled;
        UpdatedAt = updatedAt;
    }

    public void MarkFailed(DateTimeOffset updatedAt)
    {
        if (Status == OrderWorkflowStatus.Fulfilled)
        {
            return;
        }

        Status = OrderWorkflowStatus.Failed;
        UpdatedAt = updatedAt;
    }
}
