namespace Order.Api.Domain;

public sealed record OrderItemInput(string Sku, int Quantity, decimal UnitPrice);
