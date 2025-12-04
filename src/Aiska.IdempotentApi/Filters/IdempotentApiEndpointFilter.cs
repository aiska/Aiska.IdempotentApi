using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Configuration;
using Aiska.IdempotentApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aiska.IdempotentApi.Filters
{
    public class IdempotentApiEndpointFilter(IServiceProvider serviceProvider, IOptions<IdempotentApiOptions> options)
    {
        internal async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next, IdempotentRequest idempotentRequest)
        {
            ArgumentNullException.ThrowIfNull(idempotentRequest);

            var service = serviceProvider.GetRequiredService<IIdempotentApiProvider>();

            if (!service.IsValidIdempotent(context.HttpContext.Request))
                return await next(context);

            idempotentRequest.IdempotentHeader = context.HttpContext.Request.Headers[options.Value.KeyHeaderName].ToString();

            if (idempotentRequest.IdempotentHeader == string.Empty)
            {
                return TypedResults.BadRequest(service.MissingHeaderError());
            }

            object? result;

            var resultProcess = await service.ProcessIdempotentAsync(idempotentRequest);
            switch (resultProcess)
            {
                case IdempotentEnumResult.Reuse:
                    result = TypedResults.UnprocessableEntity(service.ReuseError());
                    break;
                case IdempotentEnumResult.Retried:
                    result = TypedResults.Conflict(service.RetriedError());
                    break;
                case IdempotentEnumResult.Success:
                    result = await next(context).ConfigureAwait(false);
                    await service.CacheAsync(idempotentRequest.IdempotentHeader, result);
                    break;
                case IdempotentEnumResult.Idempotent:
                    return idempotentRequest?.CacheData?.ResponseCache;
                default:
                    result = await next(context).ConfigureAwait(false);
                    break;
            }
            return result;
        }
    }
}
