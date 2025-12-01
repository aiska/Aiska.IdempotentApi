namespace Aiska.IdempotentApi.Extensions
{
    public static class IdempotentError
    {
        public const string MissingHeader = "MissingHeader";
        public const string MissingHeaderTitle = "Idempotency-Key is missing";
        public const string MissingHeaderDetail = "This operation is idempotent and it requires correct usage of Idempotency Key";
        public const string Reuse = "Reuse";
        public const string ReuseTitle = "Idempotency-Key is already used";
        public const string ReuseDetail = "This operation is idempotent and it requires correct usage of Idempotency Key. Idempotency Key MUST not be reused across different payloads of this operation.";
        public const string Retried = "Retried";
        public const string RetriedTitle = "A request is outstanding for this Idempotency-Key";
        public const string RetriedDetail = "A request with the same Idempotency-Key for the same operation is being processed or is outstanding.";
    }
}
