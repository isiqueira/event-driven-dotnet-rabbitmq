using Order.Api.Contracts;
using Order.Api.Services;

namespace Order.Api.Tests;

public sealed class CreateOrderRequestValidatorTests
{
    [Fact]
    public void Validate_returns_errors_for_missing_customer_and_empty_items()
    {
        var errors = CreateOrderRequestValidator.Validate(new CreateOrderRequest("", []));

        Assert.Contains("customerId", errors.Keys);
        Assert.Contains("items", errors.Keys);
    }

    [Fact]
    public void Validate_returns_errors_for_invalid_item_values()
    {
        var request = new CreateOrderRequest(
            "customer-001",
            [new CreateOrderItemRequest("", 0, 0m)]);

        var errors = CreateOrderRequestValidator.Validate(request);

        Assert.Contains("items[0].sku", errors.Keys);
        Assert.Contains("items[0].quantity", errors.Keys);
        Assert.Contains("items[0].unitPrice", errors.Keys);
    }
}
