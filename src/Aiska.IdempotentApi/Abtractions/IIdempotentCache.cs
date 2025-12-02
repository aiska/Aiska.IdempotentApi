using Microsoft.Extensions.Caching.Memory;

namespace Aiska.IdempotentApi.Abtractions
{
    public interface IIdempotentCache
    {
        ICacheEntry CreateEntry(string key);
        bool TryGetValue<T>(string key, out T value);
        void SetCache(string key, object? value);
    }
}
