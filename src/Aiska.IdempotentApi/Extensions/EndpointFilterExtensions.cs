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

                IdempotentRequest endpointParam = new();
                var parameters = routeHandlerContext.MethodInfo.GetParameters();
                foreach (var parameter in parameters)
                {
                    if (parameter?.Name != null)
                    {
                        IdempotentExcludeAttribute attribute = parameter.GetCustomAttribute<IdempotentExcludeAttribute>() ?? new IdempotentExcludeAttribute();
                        endpointParam.Parameters.Add(new IdempotentParamRequest
                        {
                            Name = parameter.Name,
                            Type = parameter.ParameterType,
                            Excludes = attribute.Exclude
                        });
                    }
                }

                object[] invokeArguments = [routeHandlerContext];
                return async context =>
                {
                    for (int i = 0; i < context.Arguments.Count; i++)
                    {
                        endpointParam.Parameters[i].Value = context.Arguments[i];
                    }

                    var filter = filterFactory.Invoke(context.HttpContext.RequestServices, invokeArguments);
                    return await filter.InvokeAsync(context, next, endpointParam);
                };
            });
            return builder;
        }
    }
}
