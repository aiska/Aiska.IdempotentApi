using Aiska.IdempotentApi.Abtractions;

namespace Aiska.IdempotentApi.Models
{
    internal sealed class IdempotentParameter
    {
        public IdempotentParamRequest[] Attributes { get; set; } = [];
        public double ExpirationFromMinutes { get; set; }
        public string? IdempotentHeaderName { get; set; }
        public string? ContentType { get; set; }
    }

    internal sealed record Argument
    (
        string Name,
        Type Type,
        object? Value
    );

    internal sealed record IdempotentRequest
    {
        public string IdempotentHeaderKey { get; set; } = string.Empty;
        public double ExpirationFromMinutes { get; set; }
        public string RequestData { get; set; } = string.Empty;
    }

    internal sealed record IdempotentResponse
    {
        public IdempotentEnumResult Type { get; set; }
        public int? statusCode { get; set; }
        public object? ResponseCache { get; set; }
        public string? HashValue { get; set; }
    }

    internal sealed class IdempotentParamRequest
    {
        public string Name { get; set; } = string.Empty;
        public Type Type { get; set; } = typeof(object);
        public string[] Excludes { get; set; } = [];
        public bool Ignore { get; internal set; }
    }
}
