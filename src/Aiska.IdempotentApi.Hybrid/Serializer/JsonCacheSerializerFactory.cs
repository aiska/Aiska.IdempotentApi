using Aiska.IdempotentApi.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Aiska.IdempotentApi.Serializer
{
    internal sealed class JsonCacheSerializerFactory(IOptions<JsonOptions> jsonOptions) : IHybridCacheSerializerFactory, IHybridCacheSerializer<IdempotentCacheData>
    {
        public IdempotentCacheData Deserialize(ReadOnlySequence<byte> source)
        {
            Utf8JsonReader reader = new(source);
            IdempotentCacheData? jsonResult = JsonSerializer.Deserialize<IdempotentCacheData>(ref reader, jsonOptions.Value.SerializerOptions);
            return jsonResult!;
        }

        public void Serialize(IdempotentCacheData value, IBufferWriter<byte> target)
        {
            using Utf8JsonWriter writer = new(target);

            JsonSerializer.Serialize(writer, value, jsonOptions.Value.SerializerOptions);
        }

        public bool TryCreateSerializer<T>([NotNullWhen(true)] out IHybridCacheSerializer<T>? serializer)
        {
            JsonCacheSerializer<T> factory = new(jsonOptions);
            serializer = factory;
            return serializer != null;
        }
    }
}