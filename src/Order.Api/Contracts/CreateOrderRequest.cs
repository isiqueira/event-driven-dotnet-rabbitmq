namespace Order.Api.Contracts;

public sealed record CreateOrderRequest(
    string? CustomerId,
    List<CreateOrderItemRequest>? Items);

public sealed record CreateOrderItemRequest(
    string? Sku,
    int Quantity,
    decimal UnitPrice);
