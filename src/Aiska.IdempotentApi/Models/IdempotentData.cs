namespace Aiska.IdempotentApi.Models
{
    internal sealed class IdempotentData
    {
        public string HashValue { get; set; } = string.Empty;
        public object? ResponseCache { get; set; }
    }
}
