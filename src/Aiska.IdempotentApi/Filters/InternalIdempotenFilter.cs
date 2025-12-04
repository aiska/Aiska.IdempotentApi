using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Attributes;
using Aiska.IdempotentApi.Configuration;
using Aiska.IdempotentApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.Reflection;

namespace Aiska.IdempotentApi.Filters
{
    internal sealed class InternalIdempotenFilter(IIdempotentApiProvider provider, IOptions<IdempotentApiOptions> options, IdempotentAttribute attribute) : IActionFilter, IAsyncActionFilter, IAsyncResultFilter
    {
        private string headerKey = string.Empty;
        private IdempotentEnumResult idempotentResult;
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (provider.IsValidIdempotent(context.HttpContext.Request))
            {
                if (attribute.HeaderKeyName == string.Empty) attribute.HeaderKeyName = options.Value.KeyHeaderName;

                headerKey = context.HttpContext.Request.Headers[options.Value.KeyHeaderName].ToString();

                if (headerKey == string.Empty)
                {
                    context.Result = new BadRequestObjectResult(provider.MissingHeaderError());
                }
                else
                {
                    var endpointParam = GetEndpointParam(context);
                    idempotentResult = await provider.ProcessIdempotentAsync(endpointParam).ConfigureAwait(false);
                    switch (idempotentResult)
                    {
                        case IdempotentEnumResult.Success:
                            await next().ConfigureAwait(false);
                            await provider.CacheAsync(headerKey, context.Result);
                            break;
                        case IdempotentEnumResult.Reuse:
                            context.Result = new UnprocessableEntityObjectResult(provider.ReuseError());
                            break;
                        case IdempotentEnumResult.Retried:
                            context.Result = new ConflictObjectResult(provider.RetriedError());
                            break;
                        case IdempotentEnumResult.Idempotent:
                            context.Result = (IActionResult?)endpointParam?.CacheData?.ResponseCache;
                            break;
                        default:
                            await next().ConfigureAwait(false);
                            break;
                    }
                }
            }
            else
            {
                await next().ConfigureAwait(false);
            }
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await next();
            if (idempotentResult == IdempotentEnumResult.Success)
                await provider.CacheAsync(headerKey, context.Result);
        }

        private static string[] GetExclude(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                foreach (var parameterDescriptor in controllerActionDescriptor.Parameters)
                {
                    if (parameterDescriptor is ControllerParameterDescriptor controllerParameterDescriptor)
                    {
                        var parameterInfo = controllerParameterDescriptor.ParameterInfo;
                        var attr = parameterInfo.GetCustomAttribute<IdempotentExcludeAttribute>();
                        if (attr != null)
                            return attr.Exclude;
                    }
                }
            }
            return [];
        }

        private IdempotentRequest GetEndpointParam(ActionExecutingContext context)
        {
            List<IdempotentParamRequest> parameters = [];
            foreach (var argument in context.ActionArguments)
            {
                IdempotentParamRequest idempotentParam = new()
                {
                    Name = argument.Key,
                    Value = argument.Value,
                    Excludes = GetExclude(context),
                    Type = argument.Value?.GetType() ?? typeof(object)
                };
                parameters.Add(idempotentParam);
            }
            IdempotentRequest endpointParam = new()
            {
                IdempotentHeader = headerKey,
                ContentType = context.HttpContext.Request.ContentType?.ToString() ?? MediaTypeNames.Application.Json,
                ExpirationFromMinutes = (attribute.ExpirationFromMinutes > 0) ? attribute.ExpirationFromMinutes : options.Value.ExpirationFromMinutes,
                Parameters = parameters
            };
            return endpointParam;
        }
    }
}
