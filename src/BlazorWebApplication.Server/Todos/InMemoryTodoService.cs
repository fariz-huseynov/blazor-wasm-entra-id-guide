using System.Collections.Concurrent;
using BlazorWebApplication.Shared.Todos;

namespace BlazorWebApplication.Server.Todos;

/// <summary>
/// Keeps todos in memory. Registered as a singleton, so many requests use it at the same time:
/// <see cref="ConcurrentDictionary{TKey, TValue}"/> keeps it thread-safe.
/// The data is lost when the app stops.
/// </summary>
public sealed class InMemoryTodoService : ITodoService
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _items = new();

    public InMemoryTodoService()
    {
        var now = DateTimeOffset.UtcNow;
        Add(new TodoItem(Guid.NewGuid(), "Learn how Blazor prerendering works", true, now.AddMinutes(-3)));
        Add(new TodoItem(Guid.NewGuid(), "Build the todo API", false, now.AddMinutes(-2)));
        Add(new TodoItem(Guid.NewGuid(), "Sign in with Microsoft Entra ID", false, now.AddMinutes(-1)));
    }

    public Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TodoItem> items = _items.Values.OrderBy(t => t.CreatedAt).ToList();
        return Task.FromResult(items);
    }

    public Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.TryGetValue(id, out var item) ? item : null);

    public Task<TodoItem> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var item = new TodoItem(Guid.NewGuid(), request.Title.Trim(), false, DateTimeOffset.UtcNow);
        Add(item);
        return Task.FromResult(item);
    }

    public Task<TodoItem?> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken = default)
    {
        // Retry if another request changed the same todo between our read and our write.
        while (_items.TryGetValue(id, out var current))
        {
            var updated = current with { Title = request.Title.Trim(), IsDone = request.IsDone };
            if (_items.TryUpdate(id, updated, current))
            {
                return Task.FromResult<TodoItem?>(updated);
            }
        }

        return Task.FromResult<TodoItem?>(null);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.TryRemove(id, out _));

    private void Add(TodoItem item) => _items[item.Id] = item;
}
