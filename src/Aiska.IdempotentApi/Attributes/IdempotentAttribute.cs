using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Filters;
using Aiska.IdempotentApi.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aiska.IdempotentApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class IdempotentAttribute() : Attribute, IFilterFactory
    {
        public double ExpirationFromMinutes { get; set; }
        public string? HeaderKeyName { get; set; }

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            IIdempotentApiProvider provider = serviceProvider.GetRequiredService<IIdempotentApiProvider>();
            IOptions<IdempotentApiOptions> options = serviceProvider.GetRequiredService<IOptions<IdempotentApiOptions>>();
            IOptions<JsonOptions> jsonOptions = serviceProvider.GetRequiredService<IOptions<JsonOptions>>();
            return new InternalIdempotenFilter(provider, options, jsonOptions, this);
        }

    }
}