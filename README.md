# Blazor WebAssembly + Microsoft Entra ID — a step-by-step guide

This repository builds a small **Blazor Web App** (.NET 10, Interactive WebAssembly) one step at a time.
Every step is a single git commit with a short guide page, so you can follow along or jump to any point.

## Quick start

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone <this-repo-url>
cd BlazorWebApplication
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

Open <https://localhost:7029>.

## The steps

Each step has a git tag. To see the app as it was after a step, run `git checkout step-01` (use `git checkout main` to come back).
To see exactly what a step changed, run `git diff step-01 step-02`.

| Step | What you do | Guide |
|---|---|---|
| 01 | Create the project from the Blazor Web App template | [01-create-project.md](docs/guide/01-create-project.md) |
| 02 | Add a shared project for the todo model and service interface | [02-add-shared-project.md](docs/guide/02-add-shared-project.md) |
| 03 | Replace Bootstrap with MudBlazor components and layout | [03-add-mudblazor.md](docs/guide/03-add-mudblazor.md) |
| 04 | Show the current render mode in the app bar | [04-show-render-mode.md](docs/guide/04-show-render-mode.md) |
| 05 | Add a todo list that works on the server and in the browser | [05-todos.md](docs/guide/05-todos.md) |
| 06 | Add a versioned API for other programs at `/api/v1/todos` | [06-external-api.md](docs/guide/06-external-api.md) |
| 07 | Document the external API with OpenAPI and browse it with Scalar | [07-openapi-scalar.md](docs/guide/07-openapi-scalar.md) |
| 08 | Sign in with Microsoft Entra ID and show the user in the app bar | [08-entra-sign-in.md](docs/guide/08-entra-sign-in.md) |
| 09 | Define app roles and use them to show or hide parts of the UI | [09-app-roles.md](docs/guide/09-app-roles.md) |

## Setting up Microsoft Entra ID

Steps 01–07 run without any setup. From step 08 on, you need an Entra ID tenant where you can create app registrations.

1. Register the app, add the redirect URIs and a client secret ([step 08](docs/guide/08-entra-sign-in.md#register-the-app-in-entra-id)).
2. Store the tenant ID, client ID and secret with `dotnet user-secrets` ([step 08](docs/guide/08-entra-sign-in.md#configure-the-app)).
3. Create the app roles `Reader`, `Contributor` and `Api.Writer` ([step 09](docs/guide/09-app-roles.md#create-the-app-roles-in-entra-id)).
4. Assign yourself a role in **Enterprise applications → Users and groups**, then sign out and in ([step 09](docs/guide/09-app-roles.md#create-the-app-roles-in-entra-id)).

Never commit real IDs or secrets. The repository only contains placeholders such as `<tenant-id>` and `<client-id>`.
