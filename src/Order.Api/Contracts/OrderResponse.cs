using Shared.Models.Orders;
using OrderEntity = Shared.Models.Orders.Order;

namespace Order.Api.Contracts;

public sealed record OrderResponse(
    Guid Id,
    string CustomerId,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<OrderItemResponse> Items,
    string? CorrelationId)
{
    public static OrderResponse From(OrderEntity order, string? correlationId = null)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(OrderItemResponse.From).ToArray(),
            correlationId);
    }
}

public sealed record OrderItemResponse(
    Guid Id,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice)
{
    public static OrderItemResponse From(OrderItem item)
    {
        return new OrderItemResponse(
            item.Id,
            item.Sku,
            item.Quantity,
            item.UnitPrice,
            item.TotalPrice);
    }
}
