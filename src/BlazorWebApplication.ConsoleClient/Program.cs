// A small console program that calls the protected external API (/api/v1/todos) with an Entra ID access token.
//
// Who it runs as depends on DefaultAzureCredential:
//   - AZURE_TENANT_ID, AZURE_CLIENT_ID and AZURE_CLIENT_SECRET set  -> a service principal (app-only token, roles, no scp)
//   - otherwise, after "az login"                                     -> you (user token, scp = access_as_user)
//
// Settings come from appsettings.json and can be overridden with environment variables,
// e.g. TodoApi__Scope=api://<client-id>/.default

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using BlazorWebApplication.Shared.Todos;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var baseUrl = config["TodoApi:BaseUrl"];
var scope = config["TodoApi:Scope"];

if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(scope) || scope.Contains('<'))
{
    Console.Error.WriteLine("Set TodoApi:Scope to api://<client-id>/.default with your real client ID,");
    Console.Error.WriteLine("in appsettings.json or with the environment variable TodoApi__Scope.");
    return 1;
}

// 1. Get an access token for the API.
//    ExcludeManagedIdentityCredential skips a slow check for an Azure-hosted identity on your own machine.
//    Remove it when this program runs in Azure with a managed identity.
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ExcludeManagedIdentityCredential = true,
});

Console.WriteLine($"Getting a token for {scope} ...");
AccessToken token;
try
{
    token = await credential.GetTokenAsync(new TokenRequestContext([scope]));
}
catch (AuthenticationFailedException ex)
{
    Console.Error.WriteLine("Could not get a token. Run 'az login --tenant <tenant-id> --allow-no-subscriptions' first,");
    Console.Error.WriteLine("or set AZURE_TENANT_ID, AZURE_CLIENT_ID and AZURE_CLIENT_SECRET for a service principal.");
    Console.Error.WriteLine();
    Console.Error.WriteLine(ex.Message);
    return 1;
}

PrintTokenClaims(token.Token);

// 2. Call the API with the token.
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

try
{
    Console.WriteLine("GET api/v1/todos");
    using (var response = await http.GetAsync("api/v1/todos"))
    {
        if (await CheckAsync(response))
        {
            var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>() ?? [];
            foreach (var todo in todos)
            {
                Console.WriteLine($"  [{(todo.IsDone ? "x" : " ")}] {todo.Title}");
            }
        }
    }

    Console.WriteLine();
    Console.WriteLine("POST api/v1/todos");
    var request = new CreateTodoRequest($"Created by the console client at {DateTime.Now:T}");
    using (var response = await http.PostAsJsonAsync("api/v1/todos", request))
    {
        if (await CheckAsync(response))
        {
            var created = await response.Content.ReadFromJsonAsync<TodoItem>();
            Console.WriteLine($"  Created '{created?.Title}' with id {created?.Id}");
        }
    }
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Could not reach {baseUrl}: {ex.Message}");
    Console.Error.WriteLine("Is the server running? For a certificate error, run 'dotnet dev-certs https --trust'.");
    return 1;
}

return 0;

// Prints the status code and explains 401 and 403. Returns true for a success status code.
static async Task<bool> CheckAsync(HttpResponseMessage response)
{
    Console.WriteLine($"  -> {(int)response.StatusCode} {response.StatusCode}");

    switch (response.StatusCode)
    {
        case HttpStatusCode.Unauthorized:
            Console.WriteLine("  The API did not accept the token: wrong audience or tenant, it expired,");
            Console.WriteLine("  or it is an app-only token without any roles (grant admin consent).");
            Console.WriteLine("  Check that the scope is api://<client-id>/.default for the server's ClientId.");
            return false;
        case HttpStatusCode.Forbidden:
            Console.WriteLine("  The token is valid, but it lacks a role (reading needs Reader or Api.Writer,");
            Console.WriteLine("  writing needs Api.Writer), or a user token lacks the access_as_user scope.");
            return false;
        case var status when (int)status >= 400:
            Console.WriteLine($"  {await response.Content.ReadAsStringAsync()}");
            return false;
        default:
            return true;
    }
}

// Shows what is inside the token. This only decodes the middle part (the payload); it does NOT validate it.
// Validating is the API's job.
static void PrintTokenClaims(string jwt)
{
    var parts = jwt.Split('.');
    if (parts.Length != 3)
    {
        Console.WriteLine("The token is not a JWT.");
        return;
    }

    var payload = parts[1].Replace('-', '+').Replace('_', '/');
    payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
    using var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
    var claims = json.RootElement;

    var isUserToken = claims.TryGetProperty("scp", out _);
    var kind = isUserToken ? "user (delegated) token" : "app-only token";
    Console.WriteLine();
    Console.WriteLine($"Token claims ({kind}, decoded, not validated):");

    string[] names = ["aud", "iss", "ver", "idtyp", "appid", "azp", "name", "scp", "roles"];
    foreach (var name in names)
    {
        if (claims.TryGetProperty(name, out var value))
        {
            var text = value.ValueKind == JsonValueKind.Array
                ? string.Join(", ", value.EnumerateArray().Select(v => v.GetString()))
                : value.ToString();
            Console.WriteLine($"  {name,-6} {text}");
        }
    }

    if (!claims.TryGetProperty("roles", out _))
    {
        if (isUserToken)
        {
            Console.WriteLine("  roles  (none) -> the API will answer 403. Assign a role in Enterprise applications,");
            Console.WriteLine("         then run 'az logout' and 'az login' again to get a fresh token.");
        }
        else
        {
            // Microsoft.Identity.Web rejects a token with neither scp nor roles (IDW10201), so this is a 401, not a 403.
            Console.WriteLine("  roles  (none) and no scp -> the API will answer 401 (IDW10201).");
            Console.WriteLine("         Grant Reader/Api.Writer under API permissions and click Grant admin consent.");
        }
    }

    Console.WriteLine();
}
