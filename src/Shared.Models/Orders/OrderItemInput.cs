namespace Shared.Models.Orders;

public sealed record OrderItemInput(string Sku, int Quantity, decimal UnitPrice);
