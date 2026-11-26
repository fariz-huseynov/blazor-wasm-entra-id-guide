# Step 04 — Show the current render mode

**What you'll do:** add a small badge to the app bar that shows whether the page is still prerendered on the server or already running in the browser.

## Steps

1. Create [RenderModeBadge.razor](../../src/BlazorWebApplication.Client/Components/RenderModeBadge.razor)
   in a new `Components` folder of the Client project. The key lines:

   ```razor
   <MudChip T="string" Size="Size.Small"
            Color="@(RendererInfo.IsInteractive ? Color.Success : Color.Warning)">
       @RendererInfo.Name
   </MudChip>
   ```

   `RendererInfo` is a property every component has, so you don't need `@inject`.

2. Make the folder's components available everywhere by adding one line to the Client's
   [_Imports.razor](../../src/BlazorWebApplication.Client/_Imports.razor):

   ```razor
   @using BlazorWebApplication.Client.Components
   ```

3. Put the badge in the app bar in [MainLayout.razor](../../src/BlazorWebApplication.Client/Layout/MainLayout.razor):

   ```razor
   <MudSpacer />
   <RenderModeBadge />
   ```

## Check it works

```bash
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

Open <https://localhost:7029> and reload the page. The badge shows:

1. **Static** (orange) for a moment, while you see the prerendered HTML.
2. **WebAssembly** (green) once the app has started in the browser.

Tip: in the browser's developer tools, set network throttling to "Slow 4G" to see the first state for longer.
Click **Counter** before the badge turns green: nothing happens, because the page isn't interactive yet.

You can also see the first state without a browser:

```bash
curl -sk https://localhost:7029/ | grep -o 'mud-chip-content">[A-Za-z]*'
```

This prints `mud-chip-content">Static`, because `curl` only gets the prerendered HTML.

## Why it works

`RendererInfo.Name` tells you which renderer is drawing the component right now:

| Value | Meaning |
|---|---|
| `Static` | The server renders plain HTML. Buttons don't work yet. |
| `WebAssembly` | The component runs in the browser with .NET on WebAssembly. |

`RendererInfo.IsInteractive` is `false` for `Static` and `true` for `WebAssembly`.

The same component runs twice. First the server **prerenders** it, so the page shows up fast.
Then the browser downloads the app, starts the component again and takes over the page.
Keep this in mind for the next steps: code in `OnInitializedAsync` also runs twice, once on each side.
