namespace Aiska.IdempotentApi.Models
{
    internal sealed class IdempotentModel
    {
        public string? IdempotencyKey { get; set; }
        public object? Body { get; set; }
    }
}
