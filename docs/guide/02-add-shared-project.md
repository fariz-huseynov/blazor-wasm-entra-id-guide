# Step 02 — Add a shared project

**What you'll do:** add a class library with the todo model and a service interface that both the server and the browser use.

## Steps

1. Create the class library and remove the sample class:

   ```bash
   dotnet new classlib -n BlazorWebApplication.Shared -o src/BlazorWebApplication.Shared
   rm src/BlazorWebApplication.Shared/Class1.cs
   ```

2. Add it to the solution and reference it from the other two projects:

   ```bash
   dotnet sln add src/BlazorWebApplication.Shared/BlazorWebApplication.Shared.csproj
   dotnet add src/BlazorWebApplication.Client reference src/BlazorWebApplication.Shared
   dotnet add src/BlazorWebApplication.Server reference src/BlazorWebApplication.Shared
   ```

3. Add the todo model in a `Todos` folder
   ([TodoItem.cs](../../src/BlazorWebApplication.Shared/Todos/TodoItem.cs)):

   ```csharp
   public sealed record TodoItem(Guid Id, string Title, bool IsDone, DateTimeOffset CreatedAt);
   ```

4. Add the request bodies for "create" and "update"
   ([TodoRequests.cs](../../src/BlazorWebApplication.Shared/Todos/TodoRequests.cs)):

   ```csharp
   public sealed record CreateTodoRequest(string Title);
   public sealed record UpdateTodoRequest(string Title, bool IsDone);
   ```

5. Add the service interface
   ([ITodoService.cs](../../src/BlazorWebApplication.Shared/Todos/ITodoService.cs)):

   ```csharp
   public interface ITodoService
   {
       Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);
       Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);
       Task<TodoItem> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default);
       Task<TodoItem?> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken = default);
       Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   }
   ```

   `null` and `false` mean "this todo does not exist".

## Check it works

```bash
dotnet build BlazorWebApplication.slnx
```

The build succeeds with 0 warnings and 0 errors. The app looks the same as before: nothing uses the new types yet.

## Why it works

The solution now has **three projects**:

| Project | Runs where | What it contains |
|---|---|---|
| `BlazorWebApplication.Server` | On the server | The host, and later the API and the real data store. |
| `BlazorWebApplication.Client` | In the browser | Pages and components. |
| `BlazorWebApplication.Shared` | Both | Plain C# types that both sides agree on. |

A page in the Client project runs twice: once on the server during **prerendering** and once in the browser.
Both times it asks for an `ITodoService`. In step 05 the server gives it an in-memory implementation
and the browser gives it one that calls the server's API over HTTP. The page doesn't need to know which one it got.

Records are a good fit for data you send over HTTP: they are short to write, compare by value
and serialize to JSON without extra code.
