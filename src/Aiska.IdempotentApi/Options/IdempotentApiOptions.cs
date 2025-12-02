using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Extensions;
using Aiska.IdempotentApi.Options;

namespace Aiska.IdempotentApi.Configuration
{
    public sealed class IdempotentApiOptions
    {

        public const string IdempotentApi = "IdempotentApi";

        public string KeyHeaderName { get; set; } = DefaultOptions.HeaderKeyName;

        public double ExpirationFromMinutes { get; set; } = DefaultOptions.ExpirationFromMinutes;

        public List<KeyValuePair<string, IdempotentErrorMessage>> Errors { get; set; } =
            [
                new KeyValuePair<string, IdempotentErrorMessage>(IdempotentError.MissingHeader,new(string.Empty, DefaultOptions.MissingHeaderTitle,DefaultOptions.MissingHeaderDetail)),
                new KeyValuePair<string, IdempotentErrorMessage>(IdempotentError.Reuse,new(string.Empty, DefaultOptions.ReuseTitle,DefaultOptions.ReuseDetail)),
                new KeyValuePair<string, IdempotentErrorMessage>(IdempotentError.Retried,new(string.Empty, DefaultOptions.RetriedTitle,DefaultOptions.RetriedDetail))
            ];
        public List<string> IncludeHeaders { get; internal set; } = ["Host", "Authorization", "Cookie", "Content-Type"];
    }
}
