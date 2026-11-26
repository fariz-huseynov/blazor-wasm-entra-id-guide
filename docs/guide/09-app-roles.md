# Step 09 — Use app roles in the UI

**What you'll do:** define three app roles in Entra ID, assign one to yourself, and let the roles decide what you can see and do on the Todos page.

## Steps

### Create the app roles in Entra ID

1. Open **Microsoft Entra ID → App registrations → BlazorWebApplication → App roles → Create app role**.
   Create these three roles, one at a time. Keep **Do you want to enable this app role?** ticked.

   | Display name | Allowed member types | Value | Description |
   |---|---|---|---|
   | Reader | **Both (Users/Groups + Applications)** | `Reader` | Can read todos. |
   | Contributor | **Users/Groups** | `Contributor` | Can read and change todos in the web app. |
   | Api Writer | **Both (Users/Groups + Applications)** | `Api.Writer` | Can change todos through the external API. |

   The **Value** is what the app checks. It must match exactly, including case.
   `Api.Writer` is used from step 11 on.

   <!-- screenshot: App roles page with the three roles -->

2. Assign a role to yourself. Open **Microsoft Entra ID → Enterprise applications → BlazorWebApplication → Users and groups → Add user/group**:

   | Field | Value |
   |---|---|
   | Users and groups | your own account |
   | Select a role | `Contributor` (or `Reader` to try the read-only view) |

   Click **Assign**. To have two roles, add a second assignment with the other role.

   <!-- screenshot: "Add Assignment" with a user and the Contributor role -->

3. **Sign out of the app and sign in again.** The roles are read at sign-in, so an open session doesn't see new roles.

### Use the roles in the app

4. Put the role names in one place in the Shared project
   ([AppRoles.cs](../../src/BlazorWebApplication.Shared/Auth/AppRoles.cs)), so the server and the browser use the same strings:

   ```csharp
   public const string Reader = "Reader";
   public const string Contributor = "Contributor";
   public const string ApiWriter = "Api.Writer";
   public const string ClaimType = "roles";
   ```

5. In the server's [Program.cs](../../src/BlazorWebApplication.Server/Program.cs), tell ASP.NET Core where the roles are,
   send all claims to the browser, and add an "access denied" page:

   ```csharp
   .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

   .AddMicrosoftIdentityWebApp(options =>
   {
       ...
       options.MapInboundClaims = false;
       options.TokenValidationParameters.RoleClaimType = AppRoles.ClaimType; // "roles"
   }, cookie =>
   {
       cookie.AccessDeniedPath = "/access-denied";
   });
   ```

   The page is [AccessDenied.razor](../../src/BlazorWebApplication.Client/Pages/AccessDenied.razor).

6. Protect the Todos page ([Todos.razor](../../src/BlazorWebApplication.Client/Pages/Todos.razor)).
   Readers and contributors may open it. Only contributors see the controls that change data:

   ```razor
   @attribute [Authorize(Roles = $"{AppRoles.Reader},{AppRoles.Contributor}")]

   <AuthorizeView Roles="@AppRoles.Contributor">
       <MudIconButton Icon="@Icons.Material.Filled.Delete" ... OnClick="() => DeleteAsync(todo)" />
   </AuthorizeView>
   ```

   The add box and the done check box are wrapped the same way. Readers see a read-only check box.

7. Add a **My claims** page ([Claims.razor](../../src/BlazorWebApplication.Client/Pages/Claims.razor)) that any signed-in user may open.
   It lists every claim and checks each role:

   ```razor
   @attribute [Authorize]
   ...
   IsInRole("@role"): <b>@_user.IsInRole(role)</b>
   ```

   Add a link to it in [NavMenu.razor](../../src/BlazorWebApplication.Client/Layout/NavMenu.razor), shown only when you are signed in:

   ```razor
   <AuthorizeView>
       <MudNavLink Href="claims" Icon="@Icons.Material.Filled.Badge">My claims</MudNavLink>
   </AuthorizeView>
   ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

1. Signed out, open **Todos**. You are sent to the Microsoft sign-in page.
2. Sign in with the **Contributor** role. Open **My claims**: you see a `roles` claim with `Contributor`,
   and `IsInRole("Contributor")` is `True`. Wait for the badge to show **WebAssembly**: the values stay the same.
3. Open **Todos**. You can add, tick and delete todos.
4. Change your assignment to **Reader** only, then sign out and sign in. **Todos** shows the list,
   but no add box and no delete buttons.
5. Remove all your roles, sign out and sign in. Opening **Todos** shows **Access denied**.

<!-- screenshot: My claims page with the roles chips -->

Without real `AzureAd` values in your user secrets (step 08), **Todos** and **My claims** show an error page
with "Unable to obtain configuration" (IDX20803). They need sign-in, and sign-in can't reach `<tenant-id>`,
because it is not a real tenant. The other pages still work. Do step 08's Entra ID setup first.

## Why it works

An **app role** is a named permission that you define on the app registration, such as `Reader`.
An administrator assigns it to users, groups or (from step 11) other applications.
Entra ID then puts the assigned roles into the token, in a claim called `roles`.

`RoleClaimType = "roles"` tells ASP.NET Core that this claim holds the roles.
Now `User.IsInRole("Reader")`, `[Authorize(Roles = ...)]` and `<AuthorizeView Roles="...">` all work.
This needs `MapInboundClaims = false` from step 08; otherwise the claim would be renamed to a long URI.

The browser gets the same user from the server, including the role claim type.
So the checks work during **prerendering** (the server renders the first HTML) and after WebAssembly starts.

Signed out, `[Authorize]` sends you to sign-in. Signed in but without a role, the server sends you to `/access-denied`.
When you move between pages in the browser, `AuthorizeRouteView` in [Routes.razor](../../src/BlazorWebApplication.Client/Routes.razor) does the same checks.

Hiding buttons is only for a nicer page. It doesn't protect data: anyone can still call `/api/ui/todos` directly.
Step 10 protects the API itself.
