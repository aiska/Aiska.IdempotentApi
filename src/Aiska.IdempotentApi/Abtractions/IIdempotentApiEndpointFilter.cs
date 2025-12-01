using Microsoft.AspNetCore.Http;

namespace Aiska.IdempotentApi.Abtractions
{
    public interface IIdempotentApiEndpointFilter
    {
        ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next, List<string?> parameters);
    }
}