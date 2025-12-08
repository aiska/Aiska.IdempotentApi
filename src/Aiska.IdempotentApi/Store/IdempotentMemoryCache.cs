using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Logging;
using Aiska.IdempotentApi.Models;
using Aiska.IdempotentApi.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aiska.IdempotentApi.Store
{
    internal sealed class IdempotentMemoryCache : IIdempotentCache, IDisposable
    {
        private readonly ILogger<IdempotentMemoryCache> logger;
        private readonly MemoryCache cache;
        private readonly TimeSpan absoluteExpirationRelativeToNow;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;

        public IdempotentMemoryCache(IOptions<IdempotentApiOptions> options, ILogger<IdempotentMemoryCache> logger)
        {
            this.logger = logger;
            absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.ExpirationFromMinutes);
            MemoryCacheOptions memOptions = new();
            cache = new MemoryCache(memOptions);
            cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            };

        }

        public ValueTask<IdempotentCacheData> GetOrCreateAsync(string key)
        {
            Task<IdempotentCacheData> task = Task.Run(() =>
            {
                IdempotentCacheData result;
                if (!cache.TryGetValue(key, out IdempotentCacheData? resultCache))
                {
                    // ... (Creation logic remains the same) ...
                    using ICacheEntry entry = cache.CreateEntry(key);
                    entry.SetOptions(cacheEntryOptions);
                    logger.CacheEntryCreated(key.SanitizeInput(), absoluteExpirationRelativeToNow);
                    result = new IdempotentCacheData();
                    entry.Value = result;
                }
                else
                {
                    result = resultCache!;
                }
                return result;
            });
            return new ValueTask<IdempotentCacheData>(task);
        }

        ValueTask IIdempotentCache.SetCacheAsync(string key, IdempotentCacheData value)
        {
            return new ValueTask(Task.Run(async () =>
            {
                logger.CacheEntryCreated(key.SanitizeInput(), absoluteExpirationRelativeToNow);
                cache.Set(key, value, absoluteExpirationRelativeToNow);
            }));
        }

        public ValueTask SetCacheAsync<T>(string key, IdempotentCacheData value)
        {
            return new ValueTask(Task.Run(async () =>
            {
                logger.CacheEntryCreated(key, absoluteExpirationRelativeToNow);
                cache.Set(key, value, absoluteExpirationRelativeToNow);
            }));
        }


        public void Dispose()
        {
            cache.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
