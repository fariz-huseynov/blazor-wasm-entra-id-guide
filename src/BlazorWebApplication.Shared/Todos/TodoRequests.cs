namespace BlazorWebApplication.Shared.Todos;

/// <summary>The body of a "create todo" request.</summary>
public sealed record CreateTodoRequest(string Title);

/// <summary>The body of an "update todo" request.</summary>
public sealed record UpdateTodoRequest(string Title, bool IsDone);
