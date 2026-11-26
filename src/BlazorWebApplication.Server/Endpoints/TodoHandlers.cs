using BlazorWebApplication.Shared.Todos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlazorWebApplication.Server.Endpoints;

/// <summary>
/// The request handlers for the todo endpoints. They only translate HTTP to <see cref="ITodoService"/> calls.
/// Named methods (instead of lambdas) keep the <c>Results&lt;...&gt;</c> return types easy to read.
/// </summary>
internal static class TodoHandlers
{
    public const int MaxTitleLength = 200;

    public static async Task<Ok<IReadOnlyList<TodoItem>>> GetAll(ITodoService todos, CancellationToken cancellationToken) =>
        TypedResults.Ok(await todos.GetAllAsync(cancellationToken));

    public static async Task<Results<Ok<TodoItem>, NotFound>> GetById(Guid id, ITodoService todos, CancellationToken cancellationToken) =>
        await todos.GetAsync(id, cancellationToken) is { } item
            ? TypedResults.Ok(item)
            : TypedResults.NotFound();

    public static async Task<Results<Created<TodoItem>, ValidationProblem>> Create(
        CreateTodoRequest request, ITodoService todos, HttpRequest httpRequest, CancellationToken cancellationToken)
    {
        if (ValidateTitle(request.Title) is { } problem)
        {
            return problem;
        }

        var item = await todos.CreateAsync(request, cancellationToken);

        // The new todo lives at "<this URL>/<id>", e.g. /api/ui/todos/<id>.
        var location = $"{httpRequest.PathBase}{httpRequest.Path.Value?.TrimEnd('/')}/{item.Id}";
        return TypedResults.Created(location, item);
    }

    public static async Task<Results<Ok<TodoItem>, NotFound, ValidationProblem>> Update(
        Guid id, UpdateTodoRequest request, ITodoService todos, CancellationToken cancellationToken)
    {
        if (ValidateTitle(request.Title) is { } problem)
        {
            return problem;
        }

        return await todos.UpdateAsync(id, request, cancellationToken) is { } item
            ? TypedResults.Ok(item)
            : TypedResults.NotFound();
    }

    public static async Task<Results<NoContent, NotFound>> Delete(Guid id, ITodoService todos, CancellationToken cancellationToken) =>
        await todos.DeleteAsync(id, cancellationToken)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    private static ValidationProblem? ValidateTitle(string? title)
    {
        string? error = string.IsNullOrWhiteSpace(title) ? "Title is required."
            : title.Trim().Length > MaxTitleLength ? $"Title must be {MaxTitleLength} characters or fewer."
            : null;

        return error is null
            ? null
            : TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["title"] = [error] });
    }
}
