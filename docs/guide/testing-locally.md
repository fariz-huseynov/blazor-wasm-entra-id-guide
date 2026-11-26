# Testing locally

A checklist to try every part of the app on your machine after [step 12](12-console-client.md).
It assumes the Entra ID setup from steps 08, 09 and 11 is done.

Start the server:

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

## In the browser

1. Open <https://localhost:7029>. The badge in the app bar shows **Static**, then **WebAssembly**.
2. **Counter** and **Weather** work without signing in.
3. Click **Sign in** and sign in with your tenant account. Your name appears in the app bar.
4. Open **My claims**. Check the `roles` claim and the `IsInRole` values.
5. Open **Todos**. What you can do depends on your role (see the table below).
6. Click **Sign out**. **Todos** now sends you to the sign-in page.

To try another role, change your assignment in **Enterprise applications → BlazorWebApplication → Users and groups**, then sign out and in.

## With Scalar

1. Get a token:

   ```bash
   az login --tenant <tenant-id> --allow-no-subscriptions
   az account get-access-token --scope api://<client-id>/.default --query accessToken -o tsv
   ```

2. Click **API docs (Scalar)**, paste the token into **Bearer Token**, and send requests.
   Without a token, every request gets `401`.

## With the console app

```bash
export TodoApi__Scope="api://<client-id>/.default"
dotnet run --project src/BlazorWebApplication.ConsoleClient
```

Run it once as yourself and once with the `AZURE_*` variables as the service principal ([step 12](12-console-client.md#run-it-as-a-service-principal-optional)).

## With curl

```bash
curl -k -i https://localhost:7029/api/ui/todos                       # 401: no cookie
curl -k -i https://localhost:7029/api/v1/todos                       # 401: no token
curl -k -i -H "Authorization: Bearer <token>" https://localhost:7029/api/v1/todos
```

## Who may do what

| Caller | Todos page | Change todos on the page | `GET /api/ui` | `POST /api/ui` | `GET /api/v1` | `POST /api/v1` |
|---|---|---|---|---|---|---|
| Signed out / no token | sign-in page | – | `401` | `401` | `401` | `401` |
| User, no role | Access denied | – | `403` | `403` | `403` | `403` |
| User, `Reader` | read-only | no | `200` | `403` | `200` | `403` |
| User, `Contributor` | yes | yes | `200` | `201` | `403` | `403` |
| User, `Api.Writer` | Access denied | – | `403` | `403` | `200` | `201` |
| Service principal, no role | – | – | – | – | `401` | `401` |
| Service principal, `Reader` | – | – | – | – | `200` | `403` |
| Service principal, `Api.Writer` | – | – | – | – | `200` | `201` |

`/api/ui` only accepts the sign-in cookie, and its `POST` also needs the antiforgery header that the page sends.
`/api/v1` only accepts a bearer token. A user token also needs the scope `access_as_user`.
`PUT` and `DELETE` follow the same rules as `POST`.
A service principal with no role gets `401`, not `403`: its token has neither `roles` nor `scp`, so the API rejects it as invalid.
