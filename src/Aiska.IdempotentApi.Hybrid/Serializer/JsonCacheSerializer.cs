using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Text.Json;

namespace Aiska.IdempotentApi.Serializer
{
    internal sealed class JsonCacheSerializer<T>(IOptions<JsonOptions> jsonOptions) : IHybridCacheSerializer<T>
    {
        public T Deserialize(ReadOnlySequence<byte> source)
        {
            Utf8JsonReader reader = new(source);
            return JsonSerializer.Deserialize<T>(ref reader, jsonOptions.Value.SerializerOptions)!;
        }

        public void Serialize(T value, IBufferWriter<byte> target)
        {
            using Utf8JsonWriter writer = new(target);

            JsonSerializer.Serialize(writer, value, jsonOptions.Value.SerializerOptions);
        }
    }
}