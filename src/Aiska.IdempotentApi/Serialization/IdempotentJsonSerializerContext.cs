using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Aiska.IdempotentApi.Serialization
{

    [JsonSerializable(typeof(IdempotentErrorMessage))]
    [JsonSerializable(typeof(Dictionary<string, object?>))]
    [JsonSerializable(typeof(JsonElement))]
    [JsonSerializable(typeof(JsonObject))]
    [JsonSerializable(typeof(IdempotentCacheData))]
    [JsonSerializable(typeof(HttpResult))]

    internal sealed partial class IdempotentJsonSerializerContext : JsonSerializerContext { }
}
