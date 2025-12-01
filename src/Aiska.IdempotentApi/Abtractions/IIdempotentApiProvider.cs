using Microsoft.AspNetCore.Http;

namespace Aiska.IdempotentApi.Abtractions
{
    public interface IIdempotentApiProvider
    {
        Task CacheAsync(string cacheKey, object? result);
        IdempotentErrorMessage? GetError(IdempotentResultEnum error);
        ValueTask<(IdempotentResultEnum, string, object?)> ProcessIdempotentAsync(EndpointFilterInvocationContext context);
    }
}
