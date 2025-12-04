using Aiska.IdempotentApi.Models;
using Microsoft.AspNetCore.Http;

namespace Aiska.IdempotentApi.Abtractions
{
    internal interface IIdempotentApiProvider
    {
        Task CacheAsync(string cacheKey, object? result);
        Task<IdempotentEnumResult> ProcessIdempotentAsync(IdempotentRequest request);
        IdempotentErrorMessage MissingHeaderError();
        IdempotentErrorMessage RetriedError();
        IdempotentErrorMessage ReuseError();
        bool IsValidIdempotent(HttpRequest request);
    }
}
