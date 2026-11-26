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
