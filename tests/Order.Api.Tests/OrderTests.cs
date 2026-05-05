using Order.Api.Domain;
using OrderEntity = Order.Api.Domain.Order;

namespace Order.Api.Tests;

public sealed class OrderTests
{
    [Fact]
    public void Create_calculates_item_totals_and_order_total()
    {
        var order = OrderEntity.Create(
            "customer-001",
            new[]
            {
                new OrderItemInput("SKU-001", 2, 100m),
                new OrderItemInput("SKU-002", 1, 50m)
            },
            DateTimeOffset.UtcNow);

        Assert.Equal(OrderStatus.PendingInventoryReservation, order.Status);
        Assert.Equal(250m, order.TotalAmount);
        Assert.Equal(200m, order.Items.Single(item => item.Sku == "SKU-001").TotalPrice);
        Assert.Equal(50m, order.Items.Single(item => item.Sku == "SKU-002").TotalPrice);
    }
}
