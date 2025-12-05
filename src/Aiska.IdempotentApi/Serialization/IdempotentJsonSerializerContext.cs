using Aiska.IdempotentApi.Abtractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Aiska.IdempotentApi.Serialization
{
    [JsonSerializable(typeof(IdempotentErrorMessage))]
    internal sealed partial class IdempotentJsonSerializerContext : JsonSerializerContext { }
}
