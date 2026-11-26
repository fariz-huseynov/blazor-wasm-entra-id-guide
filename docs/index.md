---
layout: page
title: Blazor WebAssembly + Microsoft Entra ID, step by step
sidebar: false
---

<HomeHero />

<div class="home-section vp-doc">

## The route

Part 1 builds a working Blazor app with a todo list and two APIs. Part 2 adds Microsoft Entra ID on top,
one concern per step. Point at a step to see what it covers.

<StepTimeline />

</div>

<div class="home-section vp-doc">

## What you end up with

<ArchDiagram />

</div>

<div class="home-section vp-doc">

## Who can do what

Three app roles, defined once in Entra ID and checked in both the web app and the APIs.
Pick a role to see what it allows.

<RolesMatrix />

</div>

<div class="home-section home-start vp-doc">

## Before you start

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). Steps 01 to 07 run without any setup.
From step 08 on, you need a Microsoft Entra ID tenant where you can create app registrations, and the
[Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) for step 11 and 12.

```bash
git clone https://github.com/fariz-huseynov/blazor-wasm-entra-id-guide.git
cd blazor-wasm-entra-id-guide
git checkout step-01      # the app as it is after step 01
dotnet run --project src/BlazorWebApplication.Server --launch-profile https
```

<a class="btn btn-primary" href="./guide/01-create-project">Start with step 01</a>

</div>
