using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Options;
using Aiska.IdempotentApi.Serialization;
using Aiska.IdempotentApi.Services;
using Aiska.IdempotentApi.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aiska.IdempotentApi.Extensions.DependencyInjection
{
    public static class IdempotentApiServiceExtensions
    {
        private static IServiceCollection IdempotentApiBase(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Add(IdempotentJsonSerializerContext.Default);
            });
            services.AddOptions<IdempotentApiOptions>();
            services.AddScoped<IIdempotentApiProvider, IdempotentApiProvider>();

            return services;
        }

        private static IServiceCollection ConfigureIdempotent(this IServiceCollection services, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            IConfigurationSection section = config.GetSection("IdempotentApi");
            services.Configure<IdempotentApiOptions>(section);
            return services;
        }

        public static IServiceCollection AddIdempotentApi(this IServiceCollection services, Action<IdempotentApiOptions> option)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(option);

            services.AddIdempotentApi();
            services.Configure(option);
            return services;
        }

        public static IServiceCollection AddIdempotentApi(this IServiceCollection services, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);

            services.AddIdempotentApi();
            services.ConfigureIdempotent(config);

            return services;
        }
        public static IServiceCollection AddIdempotentApi(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.IdempotentApiBase();

            services.AddSingleton<IIdempotentCache, IdempotentMemoryCache>();

            return services;
        }

        internal static IServiceCollection AddIdempotentApi<TCache>(this IServiceCollection services)
            where TCache : class, IIdempotentCache
        {
            ArgumentNullException.ThrowIfNull(services);

            services.IdempotentApiBase();

            services.AddSingleton<IIdempotentCache, TCache>();

            return services;
        }

        public static IServiceCollection AddIdempotentApi<TCache>(this IServiceCollection services,
            Action<IdempotentApiOptions> option)
            where TCache : class, IIdempotentCache
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(option);

            services.AddIdempotentApi<TCache>();
            services.Configure(option);

            return services;
        }
        public static IServiceCollection AddIdempotentApi<TCache>(this IServiceCollection services,
            IConfiguration config)
            where TCache : class, IIdempotentCache
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);

            services.AddIdempotentApi<TCache>();
            services.ConfigureIdempotent(config);

            return services;
        }
    }
}