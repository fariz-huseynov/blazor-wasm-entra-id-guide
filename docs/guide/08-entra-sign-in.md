# Step 08 — Sign in with Microsoft Entra ID

**What you'll do:** register the app in Microsoft Entra ID, let users sign in and out, and show who is signed in, both on the server and in WebAssembly.

## Steps

### Register the app in Entra ID

This repo uses port `7029` (see [launchSettings.json](../../src/BlazorWebApplication.Server/Properties/launchSettings.json), `https` profile).
If your own project uses another port, use it in all URLs below, or change `applicationUrl` to `https://localhost:7029`.
Otherwise sign-in fails with AADSTS50011.

1. In the [Azure portal](https://portal.azure.com), open **Microsoft Entra ID → App registrations → New registration** and enter:

   | Field | Value |
   |---|---|
   | Name | `BlazorWebApplication` |
   | Supported account types | **Accounts in this organizational directory only** (single tenant) |
   | Redirect URI — platform | **Web** |
   | Redirect URI — URL | `https://localhost:7029/signin-oidc` |

   Click **Register**.

   <!-- screenshot: "Register an application" form filled in -->

2. On the app's **Overview** page, copy two values. You need them in step 5.

   | Overview field | Used as |
   |---|---|
   | Application (client) ID | `<client-id>` |
   | Directory (tenant) ID | `<tenant-id>` |

3. Open **Authentication** (in the app registration) and set:

   | Setting | Value |
   |---|---|
   | Redirect URIs (Web) | `https://localhost:7029/signin-oidc` and `https://localhost:7029/signout-callback-oidc` |
   | Front-channel logout URL | `https://localhost:7029/signout-oidc` |
   | Implicit grant: Access tokens / ID tokens | both **unticked** |

   Click **Save**.

   <!-- screenshot: Authentication page with the two redirect URIs and the front-channel logout URL -->

4. Open **Certificates & secrets → Client secrets → New client secret**. Enter a description, click **Add**,
   and copy the **Value** right away (it is shown only once). This is `<client-secret>`.

### Configure the app

5. Store the three values with **user secrets**. They live in your user profile, not in the repository,
   so you can't commit them by accident:

   ```bash
   dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>" --project src/BlazorWebApplication.Server
   dotnet user-secrets set "AzureAd:ClientId" "<client-id>" --project src/BlazorWebApplication.Server
   dotnet user-secrets set "AzureAd:ClientSecret" "<client-secret>" --project src/BlazorWebApplication.Server
   ```

   The project already has a `UserSecretsId`. In your own project, run
   `dotnet user-secrets init --project src/BlazorWebApplication.Server` once first.

   Check them with `dotnet user-secrets list --project src/BlazorWebApplication.Server`.

6. The rest of the settings are in the server's [appsettings.json](../../src/BlazorWebApplication.Server/appsettings.json).
   The placeholders are replaced by your user secrets:

   ```json
   "AzureAd": {
     "Instance": "https://login.microsoftonline.com/",
     "TenantId": "<tenant-id>",
     "ClientId": "<client-id>",
     "CallbackPath": "/signin-oidc",
     "SignedOutCallbackPath": "/signout-callback-oidc",
     "RemoteSignOutPath": "/signout-oidc"
   }
   ```

### Add sign-in to the server

7. Add the packages:

   ```bash
   dotnet add src/BlazorWebApplication.Server package Microsoft.Identity.Web --version 4.18.0
   dotnet add src/BlazorWebApplication.Client package Microsoft.AspNetCore.Components.WebAssembly.Authentication --version 10.0.15
   ```

8. Turn on sign-in in the server's [Program.cs](../../src/BlazorWebApplication.Server/Program.cs):

   ```csharp
   builder.Services.AddRazorComponents()
       .AddInteractiveWebAssemblyComponents()
       .AddAuthenticationStateSerialization();

   builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
       .AddMicrosoftIdentityWebApp(options =>
       {
           builder.Configuration.Bind("AzureAd", options);
           options.ResponseType = "code";
           options.MapInboundClaims = false;
       });
   builder.Services.AddAuthorization();
   builder.Services.AddCascadingAuthenticationState();
   ...
   app.MapAuthenticationEndpoints();
   ```

9. Add the sign-in and sign-out URLs
   ([AuthenticationEndpoints.cs](../../src/BlazorWebApplication.Server/Endpoints/AuthenticationEndpoints.cs)):

   ```csharp
   group.MapGet("/login", (HttpRequest request, string? returnUrl) =>
       TypedResults.Challenge(GetAuthProperties(request, returnUrl)));

   group.MapPost("/logout", (HttpRequest request, [FromForm] string? returnUrl) =>
       TypedResults.SignOut(GetAuthProperties(request, returnUrl),
           [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));
   ```

   `GetAuthProperties` only accepts return URLs inside this app.

### Use it in the WebAssembly app

10. Receive the user in the Client's [Program.cs](../../src/BlazorWebApplication.Client/Program.cs):

    ```csharp
    builder.Services.AddAuthorizationCore();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddAuthenticationStateDeserialization();
    ```

    Also add `@using Microsoft.AspNetCore.Authorization` and `@using Microsoft.AspNetCore.Components.Authorization`
    to [_Imports.razor](../../src/BlazorWebApplication.Client/_Imports.razor).

11. Replace `RouteView` with `AuthorizeRouteView` in [Routes.razor](../../src/BlazorWebApplication.Client/Routes.razor).
    Signed-out users are sent to sign-in by
    [RedirectToLogin.razor](../../src/BlazorWebApplication.Client/Components/RedirectToLogin.razor):

    ```razor
    <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
        <NotAuthorized>
            @if (context.User.Identity?.IsAuthenticated != true)
            {
                <RedirectToLogin />
            }
            ...
    ```

    No page needs a signed-in user yet. Step 09 adds the first ones.

12. Add [LoginDisplay.razor](../../src/BlazorWebApplication.Client/Layout/LoginDisplay.razor) and put
    `<LoginDisplay />` in the app bar in [MainLayout.razor](../../src/BlazorWebApplication.Client/Layout/MainLayout.razor):

    ```razor
    <AuthorizeView>
        <Authorized>
            <MudText Typo="Typo.body2">@context.User.Identity?.Name</MudText>
            <form action="authentication/logout" method="post">
                <AntiforgeryToken />
                <input type="hidden" name="ReturnUrl" value="@_currentUrl" />
                <MudButton ButtonType="ButtonType.Submit">Sign out</MudButton>
            </form>
        </Authorized>
        <NotAuthorized>
            <MudButton Href="authentication/login?returnUrl=...">Sign in</MudButton>
        </NotAuthorized>
    </AuthorizeView>
    ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

1. Open <https://localhost:7029>. The app bar shows **Sign in**.
2. Click **Sign in**. You see the Microsoft sign-in page. Sign in with an account from your tenant.
   The first time, you may be asked to accept the app's permissions.
3. You are back in the app. The app bar shows your sign-in name and **Sign out**.
   The render mode badge still changes from **Static** to **WebAssembly**, and your name stays.
4. Click **Sign out**. You are signed out of the app and of Entra ID, and the app bar shows **Sign in** again.

<!-- screenshot: app bar with the user name and "Sign out" -->

If you skip the Entra ID setup, the app still starts and all pages work.
Only **Sign in** fails with an error page, because `<tenant-id>` is not a real tenant.

## Why it works

**Microsoft Entra ID** (formerly Azure Active Directory) is Microsoft's sign-in service.
An **app registration** tells Entra ID about your app: its ID, where to send users back after sign-in (the **redirect URI**),
and how the app proves who it is (the **client secret**).

The server signs the user in with **OpenID Connect**: it sends the browser to Entra ID,
gets a code back at `/signin-oidc`, and swaps the code for an **ID token** using the client secret.
The ID token says who the user is. The server then keeps the user in an encrypted **cookie**.
`Microsoft.Identity.Web` does all of this for you with `AddMicrosoftIdentityWebApp`.

The WebAssembly app never gets a token. During prerendering, the server writes the user's name and claims into the page
(`AddAuthenticationStateSerialization`), and the browser reads them back (`AddAuthenticationStateDeserialization`).
A **claim** is one fact about the user, such as their name or object ID. `<AuthorizeView>` uses them to show the right content.
This setup, where the server holds the sign-in and the browser only calls its own server, is often called a **BFF** (backend for frontend).

Sign-out is a `POST` with an **antiforgery token**: a random value that only pages of this app know.
It stops other sites from signing you out with a hidden link.
The **front-channel logout URL** is where Entra ID asks the app to sign you out when you sign out elsewhere.
Browsers that block third-party cookies (most today) may skip this, so the app cookie can stay until it expires.

`MapInboundClaims = false` keeps the claim names exactly as they are in the token (`name`, `oid`, `roles`, ...).
Step 09 relies on this for app roles.
