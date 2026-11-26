# Step 07 — Document the API with OpenAPI and Scalar

**What you'll do:** generate an OpenAPI document for `/api/v1` and browse it with the Scalar API reference page.

## Steps

1. Add two packages to the Server project:

   ```bash
   dotnet add src/BlazorWebApplication.Server package Microsoft.AspNetCore.OpenApi --version 10.0.15
   dotnet add src/BlazorWebApplication.Server package Scalar.AspNetCore --version 3.3.0
   ```

2. Register an OpenAPI document named `v1` in the server's [Program.cs](../../src/BlazorWebApplication.Server/Program.cs).
   `ShouldInclude` keeps only the external API in it:

   ```csharp
   builder.Services.AddOpenApi("v1", options =>
   {
       options.ShouldInclude = description =>
           description.RelativePath?.StartsWith("api/v1/", StringComparison.OrdinalIgnoreCase) == true;
   });
   ```

   The full file also sets the document's title with a small **document transformer**
   (a function that can change the document before it is sent).

3. Map the document and the Scalar page, only in Development:

   ```csharp
   using Scalar.AspNetCore;

   if (app.Environment.IsDevelopment())
   {
       app.MapOpenApi();               // /openapi/v1.json
       app.MapScalarApiReference(options => options.WithTitle("BlazorWebApplication API")); // /scalar
   }
   ```

4. Add a menu link in [NavMenu.razor](../../src/BlazorWebApplication.Client/Layout/NavMenu.razor):

   ```razor
   <MudNavLink Href="scalar/" Target="_blank" Icon="@Icons.Material.Filled.Api">API docs (Scalar)</MudNavLink>
   ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

1. Open <https://localhost:7029/openapi/v1.json>. You see JSON with the five `/api/v1/todos` operations
   and no `/api/ui` paths.
2. Click **API docs (Scalar)** in the menu. A new tab opens <https://localhost:7029/scalar/>.
3. Open **Todos → List all todos** and click **Test Request**, then **Send**. You get `200` and the todo list.

<!-- screenshot: Scalar page with "List all todos" opened and a 200 response -->

## Why it works

**OpenAPI** is a standard JSON format that describes an HTTP API: its URLs, parameters, request bodies and responses.
.NET 10 builds this document for you from the minimal API endpoints. The names and summaries from step 06 appear in it.
**Scalar** is a web page that reads the document and lets you read about and try every endpoint.

The UI API stays hidden for two reasons: `ExcludeFromDescription()` from step 05,
and `ShouldInclude` here, which accepts only paths that start with `api/v1/`.

The link opens in a new tab (`Target="_blank"`). The Blazor router doesn't handle such links,
so the browser asks the server for `/scalar/`. That page is not part of the Blazor app.

### Show the docs outside Development (optional)

The `if (app.Environment.IsDevelopment())` check keeps the API description off production servers.
When you run the app in another environment, `/openapi/v1.json` and `/scalar` return `404`, and the menu link leads to the "Not found" page.

To show them elsewhere, move the two `Map...` calls out of the `if`, or use your own check, for example a config setting:

```csharp
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ShowApiDocs"))
```

The document only describes the API. Protecting the API itself is the job of steps 10 and 11.
