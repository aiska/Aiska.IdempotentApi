using Aiska.IdempotentApi.Attributes;
using Aiska.IdempotentApi.Filters;
using Aiska.IdempotentApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Aiska.IdempotentApi.Extensions
{
    public static class EndpointFilterExtensions
    {
        public static TBuilder AddIdempotentFilter<TBuilder>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
        {
            Type[] types = [];
            ObjectFactory<IdempotentApiEndpointFilter> filterFactory = ActivatorUtilities.CreateFactory<IdempotentApiEndpointFilter>(types);
            builder.AddEndpointFilterFactory((routeHandlerContext, next) =>
            {

                ParameterInfo[] parameters = routeHandlerContext.MethodInfo.GetParameters();

                IdempotentParameter idempotentParameter = new()
                {
                    Attributes = [.. parameters
                        .Where(parameter => parameter.Name != null)
                        .Select(parameter =>
                        {
                            IdempotentExcludeAttribute attribute = parameter.GetCustomAttribute<IdempotentExcludeAttribute>() ?? new IdempotentExcludeAttribute();
                            IdempotentIgnoreAttribute? Ignore = parameter.GetCustomAttribute<IdempotentIgnoreAttribute>();

                            return new IdempotentParamRequest
                            {
                                Name = parameter.Name!, // Use null-forgiving operator if you are confident it's not null due to the .Where check
                                Type = parameter.ParameterType,
                                Excludes = attribute.Exclude,
                                Ignore = Ignore is not null
                            };
                        })] // Convert the resulting enumerable directly to an array
                };

                object[] invokeArguments = [routeHandlerContext];
                return async context =>
                {
                    idempotentParameter.ContentType = context.HttpContext.Request.ContentType;
                    IdempotentApiEndpointFilter filter = filterFactory.Invoke(context.HttpContext.RequestServices, invokeArguments);
                    return await filter.InvokeAsync(context, next, idempotentParameter).ConfigureAwait(false);
                };
            });
            return builder;
        }
    }
}
