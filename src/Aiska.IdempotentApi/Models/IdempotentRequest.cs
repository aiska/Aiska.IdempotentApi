using System.Net.Mime;

namespace Aiska.IdempotentApi.Models
{
    internal sealed class IdempotentRequest
    {
        public string IdempotentHeader { get; set; } = string.Empty;
        public double ExpirationFromMinutes { get; set; }
        public string ContentType { get; set; } = MediaTypeNames.Application.Json;
        public string RequestData { get; set; } = string.Empty;
        public string HashValue { get; set; } = string.Empty;
        public IdempotentData? CacheData { get; set; }

        public List<IdempotentParamRequest> Parameters { get; set; } = [];
    }

    internal sealed class IdempotentParamRequest
    {
        public string Name { get; set; } = string.Empty;
        public Type Type { get; set; } = typeof(object);
        public string[] Excludes { get; set; } = [];
        public object? Value { get; internal set; }
    }
}
