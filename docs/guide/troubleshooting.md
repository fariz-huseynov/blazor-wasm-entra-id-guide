# Troubleshooting

Common errors, what they mean, and how to fix them. `AADSTS...` codes come from Entra ID and are shown on the sign-in page or by the Azure CLI.

## Sign-in and tokens

**AADSTS50011: The redirect URI ... does not match**
The app sent a redirect URI that is not in the app registration.
Add `https://localhost:7029/signin-oidc` (and `/signout-callback-oidc`) under **App registrations → BlazorWebApplication → Authentication → Web** ([step 08](08-entra-sign-in.md#register-the-app-in-entra-id)).
Check the port and `https`.

**AADSTS65001: The user or administrator has not consented to use the application**
The Azure CLI (or another client) is not allowed to ask for a token for your API.
Add `04b07795-8ddb-461a-bbee-02f9e1bf7b46` under **Expose an API → Authorized client applications** ([step 11](11-protect-external-api.md#expose-the-api-in-entra-id)).
For a service principal: click **Grant admin consent** on its **API permissions** page ([step 12](12-console-client.md#run-it-as-a-service-principal-optional)).

**AADSTS500011: The resource principal named api://... was not found in the tenant**
The scope points to an API that Entra ID doesn't know.
Check that the **Application ID URI** is set to `api://<client-id>` ([step 11](11-protect-external-api.md#expose-the-api-in-entra-id)),
that `<client-id>` is the web app's ID, and that you are signed in to the right tenant (`az login --tenant <tenant-id> --allow-no-subscriptions`).

**Error page with IDX20803 "Unable to obtain configuration"**
`TenantId` is still the placeholder `<tenant-id>`, or the machine can't reach `login.microsoftonline.com`.
Check `dotnet user-secrets list --project src/BlazorWebApplication.Server` ([step 08](08-entra-sign-in.md#configure-the-app)).

## Roles

**No `roles` claim / `IsInRole` is `False`**
- Assign the role in **Enterprise applications → BlazorWebApplication → Users and groups** (not in App registrations) ([step 09](09-app-roles.md#create-the-app-roles-in-entra-id)).
- Sign out of the app and sign in again. Roles are read when the token is made.
- For the Azure CLI and the console app: `az account get-access-token` returns a cached token (valid for about an hour) with the old roles.
  Run `az logout`, then `az login --tenant <tenant-id> --allow-no-subscriptions`, or wait for the token to expire.
- For a service principal: add the role under **API permissions → Application permissions** and grant admin consent.
  The role must allow **Applications** as member type.
- Check what you have on the **My claims** page, or paste a token into <https://jwt.ms>.

## API answers

**401 or 403?**
- `401 Unauthorized`: the API doesn't know who you are. No token or cookie, an expired token, or a token for another audience or tenant.
- `403 Forbidden`: the API knows who you are, but you may not do this. A role is missing, or a user token lacks the scope `access_as_user`.

**`/api/v1` returns 401 in the browser, although I am signed in**
That is by design. `/api/v1` only accepts bearer tokens and ignores the sign-in cookie ([step 11](11-protect-external-api.md#why-it-works)).
Use Scalar with a token, or the console app. The web app's own pages use `/api/ui`.

**`POST /api/ui/todos` returns 400 "Invalid or missing antiforgery token"**
The request has no `RequestVerificationToken` header, or the token belongs to another user. Reload the page after signing in ([step 10](10-protect-ui-api.md#why-it-works)).

**401 with a token that looks right**
Make sure it is an **access token** for `api://<client-id>`, not an ID token, and that its `aud` is `api://<client-id>` or `<client-id>`.
An app-only token without any `roles` claim is rejected with `401`, not `403` (it has no `scp` either).
Grant the application permissions and click **Grant admin consent** ([step 12](12-console-client.md#run-it-as-a-service-principal-optional)).

## Local setup

**`az login` says "No subscriptions found"**
Your tenant has no Azure subscription (for example a free or developer tenant). That is fine for this guide.
Sign in with `az login --tenant <tenant-id> --allow-no-subscriptions`.

**The SSL connection could not be established / certificate errors**
Trust the ASP.NET Core development certificate once, then restart the server:

```bash
dotnet dev-certs https --trust
```

**The console app says "Could not get a token"**
Run `az login --tenant <tenant-id> --allow-no-subscriptions`, or set all three of `AZURE_TENANT_ID`, `AZURE_CLIENT_ID` and `AZURE_CLIENT_SECRET`.
If only some of them are set, `DefaultAzureCredential` skips them.
