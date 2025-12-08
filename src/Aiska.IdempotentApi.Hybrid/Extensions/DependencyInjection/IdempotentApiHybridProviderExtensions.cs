using Aiska.IdempotentApi.Options;
using Aiska.IdempotentApi.Serializer;
using Aiska.IdempotentApi.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aiska.IdempotentApi.Extensions.DependencyInjection
{
    public static class IdempotentApiHybridProviderExtensions
    {
        public static IServiceCollection AddIdempotentApiHybridRedis(this IServiceCollection services, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(services);


            services.AddIdempotentApi<IdempotentHybridCache>(config);
            services.AddHybridCache().AddSerializerFactory<JsonCacheSerializerFactory>();
            services.AddStackExchangeRedisCache(opt =>
                opt.Configuration = config["IdempotentApi:RedisConnection"] ?? throw new InvalidOperationException("Redis connection string not found.")
            );

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Add(HybridJsonSerializerContext.Default);
            });

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

        public static IServiceCollection AddIdempotentApiHybridRedis(this IServiceCollection services, string redisConnection)
        {
            ArgumentNullException.ThrowIfNull(services);


            services.AddIdempotentApi<IdempotentHybridCache>();
            services.AddHybridCache().AddSerializerFactory<JsonCacheSerializerFactory>();
            services.AddStackExchangeRedisCache(opt =>
                opt.Configuration = redisConnection
            );

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Add(HybridJsonSerializerContext.Default);
            });

            return services;
        }

        public static IServiceCollection AddIdempotentApiHybridRedis(this IServiceCollection services, string redisConnection, Action<IdempotentApiOptions> option)
        {
            ArgumentNullException.ThrowIfNull(services);


            services.AddIdempotentApi<IdempotentHybridCache>(option);
            services.AddHybridCache().AddSerializerFactory<JsonCacheSerializerFactory>();
            services.AddStackExchangeRedisCache(opt =>
                opt.Configuration = redisConnection
            );

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Add(HybridJsonSerializerContext.Default);
            });

            return services;
        }
    }
}
