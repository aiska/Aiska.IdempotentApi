namespace Aiska.IdempotentApi.Abtractions
{
    public enum IdempotentEnumResult
    {
        CacheMiss,
        HeaderMissing,
        Reuse,
        Retried,
        CacheHit,
        Continue
    }
}
