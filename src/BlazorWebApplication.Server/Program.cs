using BlazorWebApplication.Server.Components;
using BlazorWebApplication.Server.Endpoints;
using BlazorWebApplication.Server.Todos;
using BlazorWebApplication.Shared.Todos;
using MudBlazor.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// MudBlazor services. The server needs them too, because it prerenders the Client pages.
builder.Services.AddMudServices();

// The todo store. Client pages also get this ITodoService while the server prerenders them.
builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();

// OpenAPI document "v1" (served at /openapi/v1.json). It only describes the external API under /api/v1.
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.RelativePath?.StartsWith("api/v1/", StringComparison.OrdinalIgnoreCase) == true;

    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "BlazorWebApplication API";
        document.Info.Version = "v1";
        document.Info.Description = "The external todo API.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Show the "Not found" page for unknown URLs, but not for the API: API clients should get a plain 404.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapUiTodoEndpoints();       // /api/ui/todos: for this app's own pages
app.MapExternalTodoEndpoints(); // /api/v1/todos: for other programs

// API documentation, only while developing. See docs/guide/07-openapi-scalar.md to show it elsewhere.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // /openapi/v1.json
    app.MapScalarApiReference(options => options.WithTitle("BlazorWebApplication API")); // /scalar
}

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebApplication.Client._Imports).Assembly);

app.Run();
