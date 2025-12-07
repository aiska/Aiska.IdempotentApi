using Aiska.IdempotentApi.Models;
using Microsoft.AspNetCore.Http;

namespace Aiska.IdempotentApi.Abtractions
{
    internal interface IIdempotentApiProvider
    {
        Task SetCacheAsync(string cacheKey, IdempotentCacheData data);
        Task<IdempotentResponse> ProcessIdempotentAsync(IdempotentRequest request);
        IdempotentErrorMessage MissingHeaderError();
        IdempotentErrorMessage RetriedError();
        IdempotentErrorMessage ReuseError();
        bool IsValidIdempotent(HttpRequest request);
    }
}
