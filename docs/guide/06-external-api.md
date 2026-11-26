# Step 06 — Add an external API

**What you'll do:** add a versioned API at `/api/v1/todos` for other programs, next to the UI's own API.

## Steps

1. Map a second endpoint group in a new file
   ([ExternalTodoEndpoints.cs](../../src/BlazorWebApplication.Server/Endpoints/ExternalTodoEndpoints.cs)).
   It reuses the handlers from step 05 and adds a tag, a name and a summary to each endpoint:

   ```csharp
   var group = app.MapGroup("/api/v1/todos")
       .WithTags("Todos");

   group.MapGet("/", TodoHandlers.GetAll)
       .WithName("ListTodos")
       .WithSummary("List all todos");
   ```

   `GET /{id}`, `POST`, `PUT /{id}` and `DELETE /{id}` follow the same pattern.

2. Map it in the server's [Program.cs](../../src/BlazorWebApplication.Server/Program.cs):

   ```csharp
   app.MapUiTodoEndpoints();       // /api/ui/todos: for this app's own pages
   app.MapExternalTodoEndpoints(); // /api/v1/todos: for other programs
   ```

   Each group lives in its own file in `Endpoints/`, so `Program.cs` stays short.

3. Add sample requests in [BlazorWebApplication.Server.http](../../src/BlazorWebApplication.Server/BlazorWebApplication.Server.http):

   ```http
   @host = https://localhost:7029

   ### List all todos
   GET {{host}}/api/v1/todos
   ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

Open `BlazorWebApplication.Server.http` in Visual Studio, Rider or VS Code
(VS Code needs the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension).
Click **Send request** above each request, from top to bottom:

| Request | Expected status |
|---|---|
| List all todos | `200 OK` with three todos |
| Create a todo | `201 Created` with a `Location` header |
| Get, Mark it as done | `200 OK` |
| Delete it | `204 No Content` |
| Empty title | `400 Bad Request` |
| Unknown id | `404 Not Found` |

Or use `curl`:

```bash
curl -sk https://localhost:7029/api/v1/todos
```

A todo you create here also shows up on the Todos page: both APIs use the same `ITodoService`.

## Why it works

The app now has **two APIs** over the same data:

| API | Who calls it | Later protected with |
|---|---|---|
| `/api/ui/todos` | This app's own pages in the browser | The sign-in cookie (step 10) |
| `/api/v1/todos` | Other programs: scripts, services, the console app in step 12 | Bearer tokens (step 11) |

Keeping them apart lets each API have its own rules. The UI API can change whenever the pages change.
The `v1` API is a promise to other programs, so it has a version in the URL and is documented.

The tag, name and summary are **metadata**. They don't change what the endpoint does.
In step 07 they show up in the generated API documentation.
