namespace Aiska.IdempotentApi.Abtractions
{
    public enum IdempotentEnumResult
    {
        Success,
        HeaderMissing,
        Reuse,
        Retried,
        Idempotent,
        Continue
    }
}
