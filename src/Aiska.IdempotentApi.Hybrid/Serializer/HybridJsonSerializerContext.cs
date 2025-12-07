using Aiska.IdempotentApi.Store;
using System.Text.Json.Serialization;

namespace Aiska.IdempotentApi.Serializer
{
    [JsonSerializable(typeof(IdempotentHybridCache))]
    internal sealed partial class HybridJsonSerializerContext : JsonSerializerContext { }
}
