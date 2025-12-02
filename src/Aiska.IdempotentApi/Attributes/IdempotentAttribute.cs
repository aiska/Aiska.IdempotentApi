using Aiska.IdempotentApi.Options;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aiska.IdempotentApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method , AllowMultiple = false, Inherited = false)]
    public class IdempotentAttribute : Attribute
    {
        public double ExpirationFromMinutes { get; set; } = DefaultOptions.ExpirationFromMinutes;
        public string HeaderKeyName { get; set; } = DefaultOptions.HeaderKeyName;
        public List<string> ExcludePropertyName = [];
    }
}
