using System.Text.Json;

namespace Shared.Messaging;

public static class IntegrationEventJson
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}
