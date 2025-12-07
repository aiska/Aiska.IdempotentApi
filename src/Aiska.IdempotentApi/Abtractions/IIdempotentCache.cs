using Aiska.IdempotentApi.Models;

namespace Aiska.IdempotentApi.Abtractions
{
    public interface IIdempotentCache
    {
        ValueTask<IdempotentCacheData> GetOrCreateAsync(string key);
        ValueTask SetCacheAsync(string key, IdempotentCacheData value);

    }
}
