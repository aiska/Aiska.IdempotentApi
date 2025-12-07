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
                opt.Configuration = config["IdempotentApi:RedisConnection"]
            );

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Add(HybridJsonSerializerContext.Default);
            });

            return services;
        }
    }
}
