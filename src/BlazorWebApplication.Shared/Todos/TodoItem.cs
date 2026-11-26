namespace BlazorWebApplication.Shared.Todos;

/// <summary>A single todo. Used by the server, the browser and the API.</summary>
public sealed record TodoItem(Guid Id, string Title, bool IsDone, DateTimeOffset CreatedAt);
