using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var todoApi = app.MapGroup("/todos");

todoApi.MapGet("/", GetAllTodos);
todoApi.MapPost("/", CreateTodo);
todoApi.MapGet("/completed", GetCompleteTodos);
todoApi.MapPut("/{id}", UpdateTodo);
todoApi.MapDelete("/{id}", DeleteTodo);

static async Task<IResult> GetAllTodos(TodoDb db)
{
    List<Todo> todos = await db.Todos.ToListAsync();

    Dictionary<string, List<Todo>> response = new Dictionary<string, List<Todo>>
    {
        { "todos", todos }
    };

    return TypedResults.Ok(response);
}

static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todos/{todo.Id}", todo);
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    List<Todo> todos = await db.Todos.Where(todo => todo.IsComplete).ToListAsync();

    Dictionary<string, List<Todo>> response = new Dictionary<string, List<Todo>>
    {
        { "todos", todos }
    };

    return TypedResults.Ok(response);
}

static async Task<IResult> UpdateTodo(int id, Todo newTodo, TodoDb db)
{
    Todo? todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.IsComplete = newTodo.IsComplete;

    await db.SaveChangesAsync();

    Dictionary<string, string> response = new Dictionary<string, string>
    {
        { "message", "Todo updated" }
    };

    return TypedResults.Ok(response);
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    Todo? todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();

    Dictionary<string, string> response = new Dictionary<string, string>
    {
        { "message", "Todo deleted" }
    };

    return TypedResults.Ok(response);
}

app.Run();
