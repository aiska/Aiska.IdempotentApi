using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Cache;
using Aiska.IdempotentApi.Extensions.DependencyInjection;
using Aiska.IdempotentApi.Hybrid.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Aiska.IdempotentApi.Hybrid.Extensions.DependencyInjection
{
    public static class IdempotentApiHybridProviderExtensions
    {
        public static IServiceCollection AddIdempotentHybridApi(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddIdempotentApi<IdempotentHybridCache>();

            services.AddSingleton<IIdempotentCache, IdempotentMemoryCache>();

            return services;
        }
    }
}
