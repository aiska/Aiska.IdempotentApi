namespace Aiska.IdempotentApi.Models
{
    public sealed class IdempotentCacheData
    {
        public string? HashValue { get; set; }
        public object? ResponseCache { get; set; }
        public int? StatusCode { get; set; }
        public bool IsCompleted { get; set; }
    }
}
