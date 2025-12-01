using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aiska.IdempotentApi.Cache
{
    public sealed class IdempotentMemoryCache : IIdempotentCache
    {
        private readonly MemoryCacheEntryOptions cacheOption;
        private readonly ILogger<IdempotentMemoryCache> logger;
        private readonly MemoryCache cache;

        public IdempotentMemoryCache(IOptions<IdempotentApiOptions> options, ILogger<IdempotentMemoryCache> logger)
        {
            this.logger = logger;
            cacheOption = options.Value.CacheOptions;
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
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Cache don't have expiration time");
                }
            }
            else
            {
                var expired = cacheOption.AbsoluteExpirationRelativeToNow;
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Creating new entry with key : {key}, expiration absolut {expired}", key, expired);
                }
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

        public void Set(string key, object? value)
        {
            ArgumentNullException.ThrowIfNull(cache);
            cache.Set(key, value, cacheOption);
        }
    }
}
