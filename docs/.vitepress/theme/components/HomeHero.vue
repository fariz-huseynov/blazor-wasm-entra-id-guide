<script setup lang="ts">
import { withBase } from 'vitepress'
import { repo } from '../../steps'
import HomeScene from './HomeScene.vue'
</script>

<template>
  <section class="home-intro">
    <p class="intro-kicker">A step-by-step guide for .NET developers</p>
    <h1 class="intro-title">Build a Blazor WebAssembly app secured with Microsoft Entra ID</h1>
    <p class="intro-lede">
      Start from an empty folder and end with a working .NET&nbsp;10 app: people sign in with Entra ID,
      app roles decide what they can do, and other programs call your API with access tokens.
      Twelve small steps, each one a git commit you can read, run and compare.
    </p>
    <div class="intro-actions">
      <a class="btn btn-solid" :href="withBase('/guide/01-create-project')">Start with step 01</a>
      <a class="btn btn-outline" :href="repo" target="_blank" rel="noreferrer">Code on GitHub</a>
    </div>
  </section>

  <section class="hero blueprint" aria-label="What you will build">
    <div class="hero-inner">
      <HomeScene />
    </div>
  </section>
</template>
