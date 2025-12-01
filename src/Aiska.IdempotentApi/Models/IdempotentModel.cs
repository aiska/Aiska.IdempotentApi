namespace Aiska.IdempotentApi.Models
{
    internal class IdempotentModel
    {
        public string? IdempotencyKey { get; set; }
        public object? Body { get; set; }
    }
}
