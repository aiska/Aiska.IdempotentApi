using Aiska.IdempotentApi.Extensions;

namespace Aiska.IdempotentApi.Options
{
    public sealed class IdempotentApiOptions
    {

        public const string IdempotentApi = "IdempotentApi";

        public string KeyHeaderName { get; set; } = DefaultOptions.HeaderKeyName;

        public double ExpirationFromMinutes { get; set; } = DefaultOptions.ExpirationFromMinutes;

        public string? RedisConnection { get; set; }

        public ErrorMessage Errors { get; set; } = new();
    }

    public class ErrorMessage
    {
        public const string Errors = "Errors";

        public string MissingHeaderType { get; set; } = "";
        public string MissingHeaderTitle { get; set; } = IdempotentError.MissingHeader;
        public string MissingHeaderDetail { get; set; } = IdempotentError.MissingHeaderDetail;
        public string ReuseType { get; set; } = "";
        public string ReuseTitle { get; set; } = IdempotentError.ReuseTitle;
        public string ReuseDetail { get; set; } = IdempotentError.ReuseDetail;
        public string RetriedType { get; set; } = "";
        public string RetriedTitle { get; set; } = IdempotentError.RetriedTitle;
        public string RetriedDetail { get; set; } = IdempotentError.RetriedDetail;
    }
}
