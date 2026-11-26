using BlazorWebApplication.Server.Components;
using BlazorWebApplication.Server.Endpoints;
using BlazorWebApplication.Server.Todos;
using BlazorWebApplication.Shared.Todos;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// MudBlazor services. The server needs them too, because it prerenders the Client pages.
builder.Services.AddMudServices();

// The todo store. Client pages also get this ITodoService while the server prerenders them.
builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();

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

app.MapUiTodoEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebApplication.Client._Imports).Assembly);

app.Run();
