using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Models;
using System.Text.Json.Serialization;

namespace Aiska.IdempotentApi.Extensions
{
    [JsonSerializable(typeof(Dictionary<string, object?>))]
    [JsonSerializable(typeof(List<Tuple<string, object?>>))]
    [JsonSerializable(typeof(List<object>))]
    [JsonSerializable(typeof(List<object?>))]
    [JsonSerializable(typeof(List<KeyValuePair<string, object?>>))]
    [JsonSerializable(typeof(List<KeyValuePair<string, string>>))]
    [JsonSerializable(typeof(IEnumerable<KeyValuePair<string, object?>>))]
    [JsonSerializable(typeof(IEnumerable<KeyValuePair<string, string>>))]
    [JsonSerializable(typeof(IdempotentModel))]
    [JsonSerializable(typeof(IdempotentErrorMessage))]
    internal partial class IdempotentJsonSerializerContext : JsonSerializerContext
    {
    }
}
