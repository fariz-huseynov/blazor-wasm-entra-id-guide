namespace BlazorWebApplication.Shared.Todos;

/// <summary>
/// Reads and changes todos. The server implements it with an in-memory store;
/// the browser implements it by calling the server's API over HTTP.
/// </summary>
public interface ITodoService
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <returns>The todo, or <c>null</c> if it does not exist.</returns>
    Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TodoItem> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default);

    /// <returns>The updated todo, or <c>null</c> if it does not exist.</returns>
    Task<TodoItem?> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken = default);

    /// <returns><c>true</c> if the todo was deleted, <c>false</c> if it did not exist.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
