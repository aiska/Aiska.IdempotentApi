using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Aiska.IdempotentApi.SampleMinimalApi
{
    public record Todo
    {
        public Todo() { }
        public Todo(int id, string name) : this()
        {
            Title = name;
            Id = id;
        }
        public Todo(int id, string name, DateOnly? dueDate) : this(id, name)
        {
            DueBy = dueDate;
        }

        public int Id { get; set; }
        public string? Title { get; set; }
        public DateOnly? DueBy { get; set; } = null;
        public bool IsComplete { get; set; } = false;
    }
}
