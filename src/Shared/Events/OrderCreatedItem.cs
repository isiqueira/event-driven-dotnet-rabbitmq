namespace Shared.Events;

public sealed record OrderCreatedItem(
    string Sku,
    int Quantity);
