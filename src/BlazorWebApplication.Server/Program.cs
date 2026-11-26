using BlazorWebApplication.Server.Components;
using BlazorWebApplication.Server.Endpoints;
using BlazorWebApplication.Server.Todos;
using BlazorWebApplication.Shared.Auth;
using BlazorWebApplication.Shared.Todos;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    // Sends the signed-in user to the WebAssembly app, so <AuthorizeView> works in the browser too.
    // SerializeAllClaims sends every claim (not only name and roles), for the "My claims" page.
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// Sign-in with Microsoft Entra ID (OpenID Connect). After sign-in, the user is kept in a cookie.
// The settings come from the "AzureAd" section (appsettings.json + user secrets).
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.ResponseType = "code";     // authorization code flow (+ PKCE); needs the client secret
        options.MapInboundClaims = false;  // keep the short claim names from the token: name, oid, tid, roles, ...
        // App roles arrive in the "roles" claim. This makes User.IsInRole("Reader") and [Authorize(Roles = ...)] use it.
        options.TokenValidationParameters.RoleClaimType = AppRoles.ClaimType;
    }, cookie =>
    {
        // Signed in, but missing a role: show our page instead of the default /Account/AccessDenied.
        cookie.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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

app.MapAuthenticationEndpoints(); // /authentication/login and /authentication/logout
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
