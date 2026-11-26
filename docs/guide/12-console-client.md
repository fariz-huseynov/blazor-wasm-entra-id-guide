# Step 12 — Call the API from a console app

**What you'll do:** add a console app that gets an access token with `DefaultAzureCredential`, shows what is in it, and calls `/api/v1/todos`, first as you and then (optional) as a service principal.

## Steps

### Create the console app

1. Create the project, add it to the solution, and add the packages:

   ```bash
   dotnet new console -n BlazorWebApplication.ConsoleClient -o src/BlazorWebApplication.ConsoleClient -f net10.0
   dotnet sln BlazorWebApplication.slnx add src/BlazorWebApplication.ConsoleClient --solution-folder src
   dotnet add src/BlazorWebApplication.ConsoleClient reference src/BlazorWebApplication.Shared
   dotnet add src/BlazorWebApplication.ConsoleClient package Azure.Identity --version 1.21.0
   dotnet add src/BlazorWebApplication.ConsoleClient package Microsoft.Extensions.Configuration.Json --version 10.0.15
   dotnet add src/BlazorWebApplication.ConsoleClient package Microsoft.Extensions.Configuration.EnvironmentVariables --version 10.0.15
   ```

   The reference to the Shared project gives the console app the same `TodoItem` and `CreateTodoRequest` types as the server.

2. Add [appsettings.json](../../src/BlazorWebApplication.ConsoleClient/appsettings.json)
   (copied to the output folder, see the [project file](../../src/BlazorWebApplication.ConsoleClient/BlazorWebApplication.ConsoleClient.csproj)):

   ```json
   "TodoApi": {
     "BaseUrl": "https://localhost:7029/",
     "Scope": "api://<client-id>/.default"
   }
   ```

   Environment variables override these values: `TodoApi__BaseUrl` and `TodoApi__Scope`.

3. Write [Program.cs](../../src/BlazorWebApplication.ConsoleClient/Program.cs). The key lines:

   ```csharp
   var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
   {
       ExcludeManagedIdentityCredential = true,
   });
   var token = await credential.GetTokenAsync(new TokenRequestContext([scope]));

   http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
   using var response = await http.GetAsync("api/v1/todos");
   ```

   The program also prints the token's claims (`aud`, `iss`, `idtyp`, `appid`/`azp`, `scp`, `roles`),
   then lists the todos and creates one. It explains `401` and `403` answers.

### Run it as yourself

4. Make sure the Azure CLI is pre-authorized and you have the `Reader` and `Api.Writer` roles
   ([step 11, items 3 and 5](11-protect-external-api.md#expose-the-api-in-entra-id)).

5. Start the server in one terminal:

   ```bash
   dotnet run --project src/BlazorWebApplication.Server --launch-profile https
   ```

6. In a second terminal, sign in and run the console app:

   ```bash
   az login --tenant <tenant-id> --allow-no-subscriptions
   export TodoApi__Scope="api://<client-id>/.default"      # PowerShell: $env:TodoApi__Scope = "api://<client-id>/.default"
   dotnet run --project src/BlazorWebApplication.ConsoleClient
   ```

   `--allow-no-subscriptions` is needed when your tenant has no Azure subscription.

### Run it as a service principal (optional)

7. Register a second app for the console. Open **Microsoft Entra ID → App registrations → New registration**:

   | Field | Value |
   |---|---|
   | Name | `BlazorWebApplication Console` |
   | Supported account types | **Accounts in this organizational directory only** (single tenant) |
   | Redirect URI | leave empty |

   Copy its **Application (client) ID**. This is `<console-client-id>`.

8. In the new app, open **Certificates & secrets → Client secrets → New client secret** and copy the **Value**.
   This is `<console-client-secret>`.

9. Give it permission to call the API. Open **API permissions → Add a permission → My APIs → BlazorWebApplication → Application permissions**,
   tick `Reader` and `Api.Writer`, and click **Add permissions**. Then click **Grant admin consent for &lt;your tenant&gt;**.
   Granting admin consent needs an admin role in the tenant, such as Cloud Application Administrator or Privileged Role Administrator.

   <!-- screenshot: API permissions with Reader and Api.Writer (Application) and "Granted" status -->

   Only roles with **Allowed member types** = *Applications* or *Both* appear here. Step 09 created them that way.

10. Run the console app with three extra environment variables:

    ```bash
    export AZURE_TENANT_ID="<tenant-id>"
    export AZURE_CLIENT_ID="<console-client-id>"
    export AZURE_CLIENT_SECRET="<console-client-secret>"
    dotnet run --project src/BlazorWebApplication.ConsoleClient
    ```

    Remove them again (`unset AZURE_TENANT_ID AZURE_CLIENT_ID AZURE_CLIENT_SECRET`) to run as yourself.
    Don't put the secret in `appsettings.json`.

## Check it works

As yourself, the output looks like this (values shortened):

```text
Token claims (user (delegated) token, decoded, not validated):
  aud    api://<client-id>
  iss    https://sts.windows.net/<tenant-id>/
  appid  04b07795-8ddb-461a-bbee-02f9e1bf7b46
  scp    access_as_user
  roles  Api.Writer, Reader

GET api/v1/todos
  -> 200 OK
POST api/v1/todos
  -> 201 Created
```

`appid` (v1 tokens) or `azp` (v2 tokens) is the Azure CLI. `aud` and `iss` look different with v2 tokens; both work.

As the service principal, there is no `scp`, and `appid`/`azp` is `<console-client-id>`.
(`idtyp` = `app` only appears if you add `idtyp` as an optional access-token claim under **Token configuration**.)
Without granted roles, its token has neither `scp` nor `roles`, and the API answers `401`. Check that admin consent was granted.

With only `Reader`, the `POST` gets `403`. After changing your own roles, run `az logout` and `az login` again:
the console app uses the Azure CLI's cached token, which keeps the old roles for about an hour. Without the server running, you get a clear "Could not reach" message.

## Why it works

A **service principal** is an application's identity in your tenant. It signs in with its own client ID and secret, with no user involved.
Its token is **app-only**: it has the **application permissions** (app roles) an admin granted, and no `scp`.
A user's token is **delegated**: the app acts for the user, so it has the user's roles and the scope `access_as_user`.

`DefaultAzureCredential` tries several ways to get a token, in order, and uses the first that works.
With the three `AZURE_*` variables, it uses them (a service principal). Without them, it falls through to your `az login`.
The same code works on your machine and on a server; only the environment changes.
`ExcludeManagedIdentityCredential` skips the check for an Azure-hosted identity, which is slow on a laptop.

The scope `api://<client-id>/.default` means "every permission this caller already has for that API".
For you, that is the pre-authorized `access_as_user`. For the service principal, it is the granted app roles.

The program only decodes the token to show you what is in it. It doesn't check the signature; the API does that.

More checks for all parts of the app: [testing-locally.md](testing-locally.md). Something fails: [troubleshooting.md](troubleshooting.md).
