using Aiska.IdempotentApi.Abtractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aiska.IdempotentApi.Filters
{
    public class IdempotentApiEndpointFilter(IServiceProvider serviceProvider, ILogger<IdempotentApiEndpointFilter> logger) : IIdempotentApiEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next, List<string?> parameters)
        {
            var service = serviceProvider.GetRequiredService<IIdempotentApiProvider>();

            //TypedResults.Ok("test");
            object? result;
            var resultProcess = await service.ProcessIdempotentAsync(context);
            switch (resultProcess.Item1)
            {
                case IdempotentEnumResult.HeaderMissing:
                    result = TypedResults.BadRequest(service.GetError(resultProcess.Item1));
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Idempotent-API: Missing Idempotency-Key header.");
                    }
                    break;
                case IdempotentEnumResult.Reuse:
                    result = TypedResults.UnprocessableEntity(service.GetError(resultProcess.Item1));
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Idempotent-API: Reused Idempotency-Key detected.");
                    }
                    break;
                case IdempotentEnumResult.Retried:
                    result = TypedResults.Conflict(service.GetError(resultProcess.Item1));
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Idempotent-API: Retried request detected.");
                    }
                    break;
                case IdempotentEnumResult.Success:
                    result = await next(context).ConfigureAwait(false);
                    await service.CacheAsync(resultProcess.Item2, result);
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Idempotent-API: Successfully processed and cached result for Idempotency-Key.");
                    }
                    break;
                case IdempotentEnumResult.Idempotent:
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Idempotent-API: Returning cached result for Idempotency-Key.");
                    }
                    return resultProcess.Item3;
                default:
                    result = await next(context).ConfigureAwait(false);
                    break;
            }
            return result;
        }
    }
}
