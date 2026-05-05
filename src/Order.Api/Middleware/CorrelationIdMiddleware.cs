using Microsoft.Extensions.Primitives;

namespace Order.Api.Middleware;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";
    private const string ItemName = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request.Headers);
        context.Items[ItemName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        await next(context);
    }

    private static string ResolveCorrelationId(IHeaderDictionary headers)
    {
        return headers.TryGetValue(HeaderName, out StringValues values)
               && !StringValues.IsNullOrEmpty(values)
            ? values.ToString()
            : Guid.NewGuid().ToString("D");
    }

    public static string GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue(ItemName, out var value)
            ? value?.ToString() ?? Guid.NewGuid().ToString("D")
            : Guid.NewGuid().ToString("D");
    }
}
