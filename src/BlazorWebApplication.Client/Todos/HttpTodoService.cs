using System.Net;
using System.Net.Http.Json;
using BlazorWebApplication.Shared.Todos;

namespace BlazorWebApplication.Client.Todos;

/// <summary>
/// The browser's <see cref="ITodoService"/>: it calls the server's /api/ui/todos endpoints.
/// The URLs have no leading slash, so they are relative to HttpClient.BaseAddress (the app's base URL).
/// </summary>
public sealed class HttpTodoService(HttpClient http) : ITodoService
{
    private const string BaseUrl = "api/ui/todos";

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<TodoItem>>(BaseUrl, cancellationToken) ?? [];

    public async Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"{BaseUrl}/{id}", cancellationToken);
        return await ReadItemOrNullAsync(response, cancellationToken);
    }

    public async Task<TodoItem> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoItem>(cancellationToken))!;
    }

    public async Task<TodoItem?> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        return await ReadItemOrNullAsync(response, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    private static async Task<TodoItem?> ReadItemOrNullAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoItem>(cancellationToken);
    }
}
