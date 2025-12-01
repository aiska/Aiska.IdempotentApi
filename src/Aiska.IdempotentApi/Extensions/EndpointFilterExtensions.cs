using Aiska.IdempotentApi.Attributes;
using Aiska.IdempotentApi.Filters;
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
                var parameters = routeHandlerContext.MethodInfo.GetParameters();
                var idempotentParameters = parameters.Where(p => p.GetCustomAttributes<IdempotentKeyAttribute>().Any())
                .Select(param => param.Name);
                List<string?> idempotentParameterNames = [.. idempotentParameters];
                var props = parameters
                    .Select(p => p.ParameterType.GetProperties()
                    .Where(prop => prop.GetCustomAttributes<IdempotentKeyAttribute>().Any())
                    .Select(prop => prop.Name))
                    .SelectMany(n => n);
                idempotentParameterNames.AddRange(props);

                object[] invokeArguments = [routeHandlerContext];
                return async context =>
                {
                    var filter = filterFactory.Invoke(context.HttpContext.RequestServices, invokeArguments);
                    return await filter.InvokeAsync(context, next, idempotentParameterNames);
                };
            });
            return builder;
        }

        //public static IEndpointConventionBuilder AddIdempotentFilter(this IEndpointConventionBuilder builder)
        //{

        //    builder.AddEndpointFilterFactory((routeHandlerContext, next) =>
        //    {
        //        // We call `CreateFactory` twice here since the `CreateFactory` API does not support optional arguments.
        //        // See https://github.com/dotnet/runtime/issues/67309 for more info.
        //        ObjectFactory filterFactory;
        //        try
        //        {
        //            filterFactory = ActivatorUtilities.CreateFactory(typeof(TFilterType), new[] { typeof(EndpointFilterFactoryContext) });
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            filterFactory = ActivatorUtilities.CreateFactory(typeof(TFilterType), Type.EmptyTypes);
        //        }

        //        builder.AddEndpointFilterFactory((routeHandlerContext, next) =>
        //        {
        //            var invokeArguments = new[] { routeHandlerContext };
        //            return (context) =>
        //            {
        //                var filter = (IEndpointFilter)filterFactory.Invoke(context.HttpContext.RequestServices, invokeArguments);
        //                return filter.InvokeAsync(context, next);
        //            };
        //        });
        //        return builder;

        //        Type[] types = [typeof(IServiceProvider)];
        //        ObjectFactory<IdempotentApiEndpointFilter> filterFactory = ActivatorUtilities.CreateFactory<IdempotentApiEndpointFilter>(types);

        //        var parameters = routeHandlerContext.MethodInfo.GetParameters();
        //        var idempotentParameters = parameters.Where(p => p.GetCustomAttributes<IdempotentKeyAttribute>().Any())
        //        .Select(param => param.Name);
        //        List<string?> idempotentParameterNames = [.. idempotentParameters];
        //        var props = parameters
        //            .Select(p => p.ParameterType.GetProperties()
        //            .Where(prop => prop.GetCustomAttributes<IdempotentKeyAttribute>().Any())
        //            .Select(prop => prop.Name))
        //            .SelectMany(n => n);
        //        idempotentParameterNames.AddRange(props);

        //        object[] invokeArguments = [routeHandlerContext];
        //        //object[] invokeArguments = [];

        //        return async invocationContext =>
        //        {
        //            var filter = filterFactory.Invoke(routeHandlerContext.ApplicationServices, invokeArguments);
        //            return await filter.InvokeAsync(invocationContext, next, idempotentParameterNames);
        //        };
        //    });
        //    return builder;
        //}
    }
}
