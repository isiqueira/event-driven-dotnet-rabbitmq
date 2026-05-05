namespace Order.Worker.Messaging;

public static class RabbitMqHeaderReader
{
    public static int GetRetryCount(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue("x-retry-count", out var value) || value is null)
        {
            return 0;
        }

        return value switch
        {
            int retryCount => retryCount,
            long retryCount => checked((int)retryCount),
            short retryCount => retryCount,
            byte retryCount => retryCount,
            string retryCount when int.TryParse(retryCount, out var parsed) => parsed,
            byte[] retryCountBytes when int.TryParse(System.Text.Encoding.UTF8.GetString(retryCountBytes), out var parsed) => parsed,
            _ => 0
        };
    }

    public static string? GetCorrelationId(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue("x-correlation-id", out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string correlationId => correlationId,
            byte[] correlationIdBytes => System.Text.Encoding.UTF8.GetString(correlationIdBytes),
            _ => value.ToString()
        };
    }

    public static Dictionary<string, object?> CopyHeaders(IDictionary<string, object?>? headers)
    {
        return headers is null
            ? []
            : headers.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
