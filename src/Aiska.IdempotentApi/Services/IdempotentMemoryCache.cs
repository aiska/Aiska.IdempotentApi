using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Configuration;
using Aiska.IdempotentApi.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aiska.IdempotentApi.Services
{
    public sealed class IdempotentMemoryCache : IIdempotentCache, IDisposable
    {
        private readonly MemoryCacheEntryOptions cacheOption;
        private readonly ILogger<IdempotentMemoryCache> logger;
        private MemoryCache? cache;

        public IdempotentMemoryCache(IOptions<IdempotentApiOptions> options, ILogger<IdempotentMemoryCache> logger)
        {
            this.logger = logger;
            cacheOption = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.ExpirationFromMinutes)
            };
            var memOptions = new MemoryCacheOptions();
            cache = new MemoryCache(memOptions);
        }

        public ICacheEntry CreateEntry(string key)
        {
            ArgumentNullException.ThrowIfNull(cache);

            if (!cacheOption.AbsoluteExpirationRelativeToNow.HasValue &&
                !cacheOption.AbsoluteExpiration.HasValue &&
                !cacheOption.SlidingExpiration.HasValue)
            {
                logger.CacheExpirationEmpty();
            }
            else
            {
                var expired = cacheOption.AbsoluteExpirationRelativeToNow;
                logger.CacheEntryCreated(key, expired);
            }
            var cacheEntry = cache.CreateEntry(key).SetOptions(cacheOption);
            return cacheEntry;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            ArgumentNullException.ThrowIfNull(cache);
            var result = cache.TryGetValue(key, out T? objValue);
            value = objValue!;
            return result;
        }

        public void SetCache(string key, object? value)
        {
            ArgumentNullException.ThrowIfNull(cache);
            cache.Set(key, value, cacheOption);
        }

        public void Dispose()
        {
            cache?.Dispose();
            cache = null;
            GC.SuppressFinalize(this);
        }
    }
}
