---
layout: page
title: Blazor WebAssembly + Microsoft Entra ID, step by step
sidebar: false
---

<HomeHero />

<div class="home-section vp-doc" id="how-to-start">

## How to start

<div class="start-grid">
<div>

1. **Install the tools.** You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).
   For steps 11 and 12 you also need the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli).

2. **Get the code.**

   ```bash
   git clone https://github.com/fariz-huseynov/blazor-wasm-entra-id-guide.git
   cd blazor-wasm-entra-id-guide
   ```

3. **Go to a step.** Every step is a git tag. `step-01` is the starting point.

   ```bash
   git checkout step-01
   ```

4. **Run it** and open <https://localhost:7029>.

   ```bash
   dotnet run --project src/BlazorWebApplication.Server --launch-profile https
   ```

5. **Read the step's page, then move on** with `git checkout step-02`, and so on.
   `git diff step-01 step-02` shows exactly what a step changed.

</div>
<aside class="start-notes">

**Two ways to follow along**

- **Read and run:** check out each tag and read its page. Fastest way to understand the ideas.
- **Build it yourself:** start in an empty folder and follow the commands on each page. Compare your code with the tag when you get stuck.

**What you need from step 08**

A Microsoft Entra ID tenant where you can create an app registration. A free tenant works.
Steps 01 to 07 need nothing but .NET.

</aside>
</div>

</div>

<div class="home-section vp-doc">

## The 12 steps

<StepList />

</div>
