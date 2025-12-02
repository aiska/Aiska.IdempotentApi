using Microsoft.AspNetCore.Http;

namespace Aiska.IdempotentApi.Abtractions
{
    public interface IIdempotentApiProvider
    {
        Task CacheAsync(string cacheKey, object? result);
        IdempotentErrorMessage? GetError(IdempotentEnumResult error);
        ValueTask<(IdempotentEnumResult, string, object?)> ProcessIdempotentAsync(EndpointFilterInvocationContext context);
    }
}
