using Aiska.IdempotentApi.Abtractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aiska.IdempotentApi.Filters
{
    public class IdempotentApiEndpointFilter(IServiceProvider serviceProvider)
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next, List<string?> parametersList)
        {
            var service = serviceProvider.GetRequiredService<IIdempotentApiProvider>();

            //TypedResults.Ok("test");
            object? result;
            var resultProcess = await service.ProcessIdempotentAsync(context);
            switch (resultProcess.Item1)
            {
                case IdempotentEnumResult.HeaderMissing:
                    result = TypedResults.BadRequest(service.GetError(resultProcess.Item1));
                    break;
                case IdempotentEnumResult.Reuse:
                    result = TypedResults.UnprocessableEntity(service.GetError(resultProcess.Item1));
                    break;
                case IdempotentEnumResult.Retried:
                    result = TypedResults.Conflict(service.GetError(resultProcess.Item1));
                    break;
                case IdempotentEnumResult.Success:
                    result = await next(context).ConfigureAwait(false);
                    await service.CacheAsync(resultProcess.Item2, result);
                    break;
                case IdempotentEnumResult.Idempotent:
                    return resultProcess.Item3;
                default:
                    result = await next(context).ConfigureAwait(false);
                    break;
            }
            return result;
        }
    }
}
