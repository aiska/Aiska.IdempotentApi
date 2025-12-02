using Aiska.IdempotentApi.Abtractions;
using Microsoft.Extensions.Caching.Memory;

namespace Aiska.IdempotentApi.Hybrid.Cache
{
    public class IdempotentHybridCache : IIdempotentCache
    {
        public ICacheEntry CreateEntry(string key)
        {
            // TODO Create new cache entry
            throw new NotImplementedException();
        }

        public void SetCache(string key, object? value)
        {
            // TODO Create set cache entry
            throw new NotImplementedException();
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            // TODO Create TryGetValue cache entry
            throw new NotImplementedException();
        }
    }
}
