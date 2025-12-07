using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Models;
using Aiska.IdempotentApi.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Aiska.IdempotentApi.Store
{
    internal sealed class IdempotentHybridCache : IIdempotentCache
    {
        private readonly TimeSpan absoluteExpirationRelativeToNow;
        private readonly HybridCacheEntryOptions cacheEntryOptions;
        private readonly HybridCache cache;
        private readonly IDistributedCache distributedCache;

        public IdempotentHybridCache(HybridCache cache, IDistributedCache distributedCache, IOptions<IdempotentApiOptions> options)
        {
            this.cache = cache;
            this.distributedCache = distributedCache;
            absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.ExpirationFromMinutes);
            cacheEntryOptions = new HybridCacheEntryOptions()
            {
                LocalCacheExpiration = absoluteExpirationRelativeToNow,
                Expiration = absoluteExpirationRelativeToNow,
            };
        }

        async ValueTask<IdempotentCacheData> IIdempotentCache.GetOrCreateAsync(string key)
        {
            return await cache.GetOrCreateAsync(key, CreateCacheEntryAsync, cacheEntryOptions).ConfigureAwait(false);
        }

        private async ValueTask<IdempotentCacheData> CreateCacheEntryAsync(CancellationToken token)
        {
            return new IdempotentCacheData();
        }

        ValueTask IIdempotentCache.SetCacheAsync(string key, IdempotentCacheData value)
        {
            return cache.SetAsync(key, value, cacheEntryOptions);
        }
    }
}
