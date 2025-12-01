using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace Aiska.IdempotentApi.Configuration
{
    public sealed class IdempotentApiOptions
    {

        public const string IdempotentApi = "IdempotentApi";

        public long MaximumBodySize { get; set; } = 64 * 1024 * 1024;

        public TimeSpan CacheLock { get; set; } = TimeSpan.FromSeconds(3);

        public string KeyPrefix { get; set; } = "UnAlt_";

        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public bool CacheBodyOnly { get; set; } = false;
        public long SizeLimit { get; set; }
        public MemoryCacheEntryOptions CacheOptions { get; set; } = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

        public List<KeyValuePair<string, IdempotentErrorMessage>> Errors { get; set; } =
            [
                new KeyValuePair<string, IdempotentErrorMessage>(IdempotentError.MissingHeader,new(string.Empty, IdempotentError.MissingHeaderTitle,IdempotentError.MissingHeaderDetail)),
                new KeyValuePair<string, IdempotentErrorMessage>(IdempotentError.Reuse,new(string.Empty, IdempotentError.ReuseTitle,IdempotentError.ReuseDetail)),
                new KeyValuePair<string, IdempotentErrorMessage>(IdempotentError.Retried,new(string.Empty, IdempotentError.RetriedTitle,IdempotentError.RetriedDetail))
            ];
        public List<string> IncludeHeaders { get; internal set; } = ["Host", "Authorization", "Cookie", "Content-Type"];
    }
}
