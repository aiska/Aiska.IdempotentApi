using Aiska.IdempotentApi.Attributes;

namespace Aiska.IdempotentApi.SampleMinimalApi
{
    public record Todo(
        [property: IdempotentKey] int Id,
        string? Title,
        DateOnly? DueBy = null,
        bool IsComplete = false
    );
}
