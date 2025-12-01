namespace Aiska.IdempotentApi.Abtractions
{
    public enum IdempotentResultEnum
    {
        Success,
        HeaderMissing,
        Reuse,
        Retried,
        Idempotent,
        Continue
    }
}
