# Step 05 — Add a todo list

**What you'll do:** add a Todos page that reads and changes todos through the same interface on the server and in the browser.

## Steps

1. Add an in-memory store on the server
   ([InMemoryTodoService.cs](../../src/BlazorWebApplication.Server/Todos/InMemoryTodoService.cs)).
   It implements `ITodoService` from step 02, keeps todos in a `ConcurrentDictionary` and starts with three sample todos.

2. Add the request handlers ([TodoHandlers.cs](../../src/BlazorWebApplication.Server/Endpoints/TodoHandlers.cs)).
   Each one calls `ITodoService` and returns a typed result:

   ```csharp
   public static async Task<Results<Ok<TodoItem>, NotFound>> GetById(Guid id, ITodoService todos, CancellationToken cancellationToken) =>
       await todos.GetAsync(id, cancellationToken) is { } item
           ? TypedResults.Ok(item)
           : TypedResults.NotFound();
   ```

   `Create` and `Update` return `400 Bad Request` when the title is empty.

3. Map the endpoints for the UI ([UiTodoEndpoints.cs](../../src/BlazorWebApplication.Server/Endpoints/UiTodoEndpoints.cs)):

   ```csharp
   var group = app.MapGroup("/api/ui/todos")
       .ExcludeFromDescription();

   group.MapGet("/", TodoHandlers.GetAll);
   group.MapGet("/{id:guid}", TodoHandlers.GetById);
   group.MapPost("/", TodoHandlers.Create);
   group.MapPut("/{id:guid}", TodoHandlers.Update);
   group.MapDelete("/{id:guid}", TodoHandlers.Delete);
   ```

4. Register the store and the endpoints in the server's [Program.cs](../../src/BlazorWebApplication.Server/Program.cs):

   ```csharp
   builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();
   ...
   app.MapUiTodoEndpoints();
   ```

   In the same file, the "Not found" page now skips URLs that start with `/api`,
   so an API client gets a plain `404` instead of an HTML page.

5. Add the browser's implementation ([HttpTodoService.cs](../../src/BlazorWebApplication.Client/Todos/HttpTodoService.cs)).
   It calls the endpoints with `HttpClient`:

   ```csharp
   public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
       await http.GetFromJsonAsync<List<TodoItem>>("api/ui/todos", cancellationToken) ?? [];
   ```

6. Register it in the Client's [Program.cs](../../src/BlazorWebApplication.Client/Program.cs):

   ```csharp
   builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
   builder.Services.AddScoped<ITodoService, HttpTodoService>();
   ```

7. Add the page ([Todos.razor](../../src/BlazorWebApplication.Client/Pages/Todos.razor)).
   It has a text field, a list with a check box and a delete button per todo. The key lines:

   ```razor
   @inject ITodoService TodoService

   @code {
       [PersistentState]
       public List<TodoItem>? Items { get; set; }

       protected override async Task OnInitializedAsync()
       {
           Items ??= [.. await TodoService.GetAllAsync()];
       }
   }
   ```

8. Add a menu link in [NavMenu.razor](../../src/BlazorWebApplication.Client/Layout/NavMenu.razor):

   ```razor
   <MudNavLink Href="todos" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.Checklist">Todos</MudNavLink>
   ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

Open <https://localhost:7029/todos>. You see three todos. Add one, tick one and delete one.

![Adding, ticking and deleting a todo](images/todos-contributor.gif)

Open the browser's developer tools on the **Network** tab and reload the page.
There is **no** request to `api/ui/todos`: the list came with the HTML.
Now click **Home** and then **Todos** again. This time you see a `GET api/ui/todos` request.

You can call the API directly too:

```bash
curl -sk https://localhost:7029/api/ui/todos
curl -sk -X POST https://localhost:7029/api/ui/todos -H "Content-Type: application/json" -d '{"title":"From curl"}'
```

The todos live in memory, so they are back to the three samples when you restart the app.

## Why it works

The page asks for an `ITodoService` and gets a different one on each side:

| Where the page runs | `ITodoService` | Where the data comes from |
|---|---|---|
| Server, during prerendering | `InMemoryTodoService` | Directly from memory |
| Browser, after the app starts | `HttpTodoService` | `GET /api/ui/todos` |

Every service a Client page injects must be registered on **both** sides. Without the server registration, prerendering fails with "There is no registered service of type ITodoService".

**Prerendering** runs `OnInitializedAsync` on the server first, and then again in the browser.
Without help, the list would load twice. `[PersistentState]` fixes this: the server writes `Items` as JSON
into a comment in the HTML, and the browser reads it back. `Items` is already set, so `??=` skips the second load.
Only data you don't mind the user seeing belongs there: anyone can read it in the page source.

After the app has started, moving between pages happens in the browser and nothing is prerendered.
That's when `HttpTodoService` does the work.

The inputs are disabled while `RendererInfo.IsInteractive` is `false`, because clicks do nothing before the app has started.

The `/api/ui` endpoints exist only for this app's pages. `ExcludeFromDescription()` keeps them out of the API documentation you add in step 07.
In step 06 you add a separate API for other programs.
