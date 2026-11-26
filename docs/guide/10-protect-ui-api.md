# Step 10 — Protect the UI API

**What you'll do:** make `/api/ui/todos` accept only signed-in users with the right role, return `401`/`403` instead of redirects, and check an antiforgery token on every change.

## Steps

1. Put the authorization policies in one place, [Auth/Policies.cs](../../src/BlazorWebApplication.Server/Auth/Policies.cs).
   Both only accept the sign-in cookie:

   ```csharp
   .AddPolicy(UiRead, policy => policy
       .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
       .RequireAuthenticatedUser()
       .RequireRole(AppRoles.Reader, AppRoles.Contributor))
   .AddPolicy(UiWrite, policy => policy
       .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
       .RequireAuthenticatedUser()
       .RequireRole(AppRoles.Contributor));
   ```

2. Register them in the server's [Program.cs](../../src/BlazorWebApplication.Server/Program.cs) instead of `AddAuthorization()`.
   In the cookie options, answer API calls with a status code instead of a redirect:

   ```csharp
   builder.Services.AddAuthorizationBuilder()
       .AddAppPolicies();

   // inside AddMicrosoftIdentityWebApp(..., cookie => { ... })
   cookie.Events.OnRedirectToLogin = context => ApiStatusOrRedirect(context, StatusCodes.Status401Unauthorized);
   cookie.Events.OnRedirectToAccessDenied = context => ApiStatusOrRedirect(context, StatusCodes.Status403Forbidden);
   ```

   `ApiStatusOrRedirect` (at the end of `Program.cs`) sets the status code for paths under `/api`
   and keeps the normal redirect for pages.

3. Add an endpoint filter that checks the antiforgery token on `POST`, `PUT` and `DELETE`
   ([AntiforgeryEndpointFilter.cs](../../src/BlazorWebApplication.Server/Endpoints/AntiforgeryEndpointFilter.cs)):

   ```csharp
   if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsOptions(method)
       && !await antiforgery.IsRequestValidAsync(http))
   {
       return TypedResults.Problem("Invalid or missing antiforgery token.", statusCode: StatusCodes.Status400BadRequest);
   }
   ```

4. Use the policies and the filter in [UiTodoEndpoints.cs](../../src/BlazorWebApplication.Server/Endpoints/UiTodoEndpoints.cs):

   ```csharp
   var group = app.MapGroup("/api/ui/todos")
       .ExcludeFromDescription()
       .RequireAuthorization(Policies.UiRead)
       .AddEndpointFilter<AntiforgeryEndpointFilter>();

   group.MapGet("/", TodoHandlers.GetAll);
   group.MapPost("/", TodoHandlers.Create).RequireAuthorization(Policies.UiWrite);
   ```

5. In the browser, send the token in a header. Add
   [AntiforgeryHandler.cs](../../src/BlazorWebApplication.Client/Todos/AntiforgeryHandler.cs) to the Client:

   ```csharp
   if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head
       && antiforgery.GetAntiforgeryToken() is { } token)
   {
       request.Headers.TryAddWithoutValidation("RequestVerificationToken", token.Value);
   }
   ```

   Use it for the `HttpClient` in the Client's [Program.cs](../../src/BlazorWebApplication.Client/Program.cs):

   ```csharp
   builder.Services.AddScoped(sp =>
       new HttpClient(new AntiforgeryHandler(sp.GetRequiredService<AntiforgeryStateProvider>())
       {
           InnerHandler = new HttpClientHandler(),
       })
       {
           BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
       });
   ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

1. Without signing in, call the API. You get `401`, not a redirect:

   ```bash
   curl -k -i https://localhost:7029/api/ui/todos
   ```

2. Sign in as a **Contributor** and open **Todos**. Add, tick and delete a todo. It works as before.
   In the browser's developer tools (**Network** tab), the `POST` request has a `RequestVerificationToken` header.
3. Sign in as a **Reader** only. Open <https://localhost:7029/api/ui/todos> in the same browser: you see the JSON list.
   A `POST` from this user would get `403`.
4. A `POST` without the header gets `400`, even for a Contributor.

The external API `/api/v1/todos` is still open. Step 11 protects it.

## Why it works

An **authorization policy** is a named set of rules, such as "signed in and has the Contributor role".
Endpoints refer to it by name, so the rules live in one file.
The group needs `UiRead`; the write endpoints also need `UiWrite`, so both must pass.

The app's default sign-in scheme is OpenID Connect. Its answer to "who are you?" is a redirect to the Microsoft sign-in page.
That is right for a page, but a `fetch` from WebAssembly can't use it.
`AddAuthenticationSchemes(Cookies)` makes the cookie handler answer instead, and the cookie events turn its redirects into `401` (not signed in) and `403` (signed in, but no permission) for `/api` paths.
It also means the UI API accepts only the cookie. Step 11 uses the same idea for bearer tokens.

The browser sends cookies with every request to this site, even when another site starts the request.
The **antiforgery token** stops that kind of attack (CSRF): the server checks that the header matches the antiforgery cookie,
and only pages of this app can read the token.
Minimal APIs check it by themselves only for form posts, so JSON endpoints need the filter.
The server writes the token into the page during prerendering; `AntiforgeryStateProvider` reads it in WebAssembly.
