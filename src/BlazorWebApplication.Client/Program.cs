using BlazorWebApplication.Client.Todos;
using BlazorWebApplication.Shared.Todos;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// The signed-in user comes from the server (AddAuthenticationStateSerialization there).
// The browser never sees a token: it only gets the user's claims.
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// In the browser, todos come from the server's API.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ITodoService, HttpTodoService>();

await builder.Build().RunAsync();
