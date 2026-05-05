using Order.Api.Contracts;

namespace Order.Api.Services;

public static class CreateOrderRequestValidator
{
    public static Dictionary<string, string[]> Validate(CreateOrderRequest? request)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (request is null)
        {
            AddError(errors, "request", "Request body is required.");
            return ToValidationProblem(errors);
        }

        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            AddError(errors, "customerId", "CustomerId is required.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            AddError(errors, "items", "At least one item is required.");
            return ToValidationProblem(errors);
        }

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];
            var prefix = $"items[{index}]";

            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                AddError(errors, $"{prefix}.sku", "Sku is required.");
            }

            if (item.Quantity <= 0)
            {
                AddError(errors, $"{prefix}.quantity", "Quantity must be greater than zero.");
            }

            if (item.UnitPrice <= 0)
            {
                AddError(errors, $"{prefix}.unitPrice", "UnitPrice must be greater than zero.");
            }
        }

        return ToValidationProblem(errors);
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }

    private static Dictionary<string, string[]> ToValidationProblem(Dictionary<string, List<string>> errors)
    {
        return errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }
}
