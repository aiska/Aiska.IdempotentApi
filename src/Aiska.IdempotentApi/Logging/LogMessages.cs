using Microsoft.Extensions.Logging;

namespace Aiska.IdempotentApi.Logging
{
    internal static partial class LogMessages
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Request is not valid for idempotency processing.")]
        public static partial void InvalidIndempotentRequest(this ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Idempotency-Key header is missing.")]
        public static partial void MissingIdempotencyKeyHeader(this ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Idempotency-Key: {IdempotencyKey} - Cache miss, proceeding to execute action.")]
        public static partial void IdempotencyKeyCacheMiss(this ILogger logger, string IdempotencyKey);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Idempotency-Key: {IdempotencyKey} - Reuse detected with different request data.")]
        public static partial void IdempotencyKeyReuse(this ILogger logger, string IdempotencyKey);

        [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Idempotency-Key: {IdempotencyKey} - Retried request detected.")]
        public static partial void IdempotencyKeyRetried(this ILogger logger, string IdempotencyKey);

        [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Idempotency-Key: {IdempotencyKey}, Cache hit, return result from cache.")]
        public static partial void IdempotencyKeyCacheHit(this ILogger logger, string IdempotencyKey);

        [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "Cache don't have expiration time. update ExpirationFromMinutes in config")]
        public static partial void CacheExpirationEmpty(this ILogger logger);

        [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Creating new cache entry key : {key}, expired in {expired}")]
        public static partial void CacheEntryCreated(this ILogger logger, string key, TimeSpan? expired);
    }
}
