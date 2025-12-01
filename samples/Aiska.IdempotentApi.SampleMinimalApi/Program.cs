using Aiska.IdempotentApi;
using Aiska.IdempotentApi.Attributes;
using Aiska.IdempotentApi.Extensions;
using Aiska.IdempotentApi.Extensions.DependencyInjection;
using Aiska.IdempotentApi.SampleMinimalApi;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddIdempotentApi(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

Todo[] sampleTodos =
[
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
];

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos)
        .WithName("GetTodos")
        .AddIdempotentFilter();

todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> ([IdempotentKey] int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound())
        .WithName("GetTodoById")
        .AddIdempotentFilter();

todosApi.MapPost("/", async (Todo todo) =>
{
    //simulate creation
    await Task.Delay(10 * 1000);
    return Results.Created($"/todoitems/{todo.Id}", todo);
})
    .WithName("CreateTodo")
    .AddIdempotentFilter();

todosApi.MapPost("/form", async ([FromForm][IdempotentKey] int id, [FromForm] string Title, [FromForm] DateOnly? DueDate) =>
{
    var todo = new Todo(id, Title, DueDate);
    //simulate creation
    await Task.Delay(10 * 1000);
    return Results.Created($"/todoitems/{todo.Id}", todo);
})
    .WithName("CreateTodoForm")
    .DisableAntiforgery()
    .AddIdempotentFilter();

app.Run();


[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
