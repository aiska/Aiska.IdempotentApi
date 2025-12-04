using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Configuration;
using Aiska.IdempotentApi.Filters;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aiska.IdempotentApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class IdempotentAttribute() : Attribute, IFilterFactory
    {
        public double ExpirationFromMinutes { get; set; }
        public string HeaderKeyName { get; set; } = string.Empty;

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var provider = serviceProvider.GetRequiredService<IIdempotentApiProvider>();
            var options = serviceProvider.GetRequiredService<IOptions<IdempotentApiOptions>>();
            return new InternalIdempotenFilter(provider, options, this);
        }

    }
}