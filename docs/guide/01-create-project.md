# Step 01 — Create the project

**What you'll do:** create a Blazor Web App that runs its UI in the browser with WebAssembly.

## Steps

1. Create a folder and a git repository:

   ```bash
   mkdir BlazorWebApplication && cd BlazorWebApplication
   git init
   ```

2. Create the app from the template:

   ```bash
   dotnet new blazor -n BlazorWebApplication --interactivity WebAssembly --all-interactive -o src
   ```

   | Option | Meaning |
   |---|---|
   | `--interactivity WebAssembly` | Interactive components run in the browser. |
   | `--all-interactive` | Every page is interactive, not only the ones you mark. |
   | `-o src` | Put the projects in a `src` folder. |

3. The template names the server project just `BlazorWebApplication`. Rename it to `BlazorWebApplication.Server`,
   so the three projects in this repo read clearly as `.Server`, `.Client` and (from step 02) `.Shared`:

   ```bash
   mv src/BlazorWebApplication src/BlazorWebApplication.Server
   mv src/BlazorWebApplication.Server/BlazorWebApplication.csproj src/BlazorWebApplication.Server/BlazorWebApplication.Server.csproj
   ```

   Then update the namespaces that pointed at the old name:

   | File | Change |
   |---|---|
   | `Program.cs` | `using BlazorWebApplication.Components;` → `using BlazorWebApplication.Server.Components;` |
   | `Components/_Imports.razor` | `@using BlazorWebApplication` → `@using BlazorWebApplication.Server`, and `.Components` the same way |
   | `Components/App.razor` | `BlazorWebApplication.styles.css` → `BlazorWebApplication.Server.styles.css` |

4. The template also creates `src/BlazorWebApplication.sln`. Delete it and create a solution in the root folder instead:

   ```bash
   rm src/BlazorWebApplication.sln
   dotnet new sln -n BlazorWebApplication --format slnx
   dotnet sln add src/BlazorWebApplication.Server/BlazorWebApplication.Server.csproj src/BlazorWebApplication.Client/BlazorWebApplication.Client.csproj
   ```

5. Add a `.gitignore` and pin the SDK version, so everyone builds with the same .NET:

   ```bash
   dotnet new gitignore
   dotnet new globaljson --sdk-version 10.0.401 --roll-forward latestFeature
   ```

6. The template may reference an older patch of the Blazor packages. Update both `.csproj` files to the latest
   `10.0.x` version (this repo uses `10.0.15`) to pick up security fixes.

7. The template picks random ports. This guide uses `https://localhost:7029` everywhere (from step 08 also in Entra ID).
   In [launchSettings.json](../../src/BlazorWebApplication.Server/Properties/launchSettings.json), set the `https` profile to:

   ```json
   "applicationUrl": "https://localhost:7029;http://localhost:5195",
   ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

Open <https://localhost:7029>. Click **Counter** and press the button: the number goes up.

## Why it works

The template creates **two projects**:

| Project | Runs where | What it contains |
|---|---|---|
| `BlazorWebApplication.Server` | On the server | The host. It sends the first HTML to the browser and serves the files for the app. |
| `BlazorWebApplication.Client` | In the browser | Pages and components. They are compiled to WebAssembly and run in the browser. |

When you open a page, the server first renders it as plain HTML (this is called **prerendering**), so you see content quickly.
Then the browser downloads .NET and the app, and the page becomes interactive.
