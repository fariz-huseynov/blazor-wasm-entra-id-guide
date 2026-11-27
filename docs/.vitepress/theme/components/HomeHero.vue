<script setup lang="ts">
import { withBase } from 'vitepress'
import { repo } from '../../steps'
import HomeScene from './HomeScene.vue'
</script>

<template>
  <section class="hero blueprint">
    <div class="hero-inner">
      <div class="hero-copy">
        <p class="hero-kicker">A step-by-step guide for .NET developers</p>
        <h1 class="hero-title">Build a Blazor WebAssembly app secured with Microsoft Entra ID</h1>
        <p class="hero-lede">
          Start from an empty folder and end with a working .NET&nbsp;10 app: people sign in with Entra ID,
          app roles decide what they can do, and other programs call your API with access tokens.
          Twelve small steps, each one a git commit you can read, run and compare.
        </p>
        <div class="hero-actions">
          <a class="btn btn-primary" :href="withBase('/guide/01-create-project')">Start with step 01</a>
          <a class="btn btn-ghost" href="#how-to-start">How to start</a>
          <a class="btn btn-ghost" :href="repo" target="_blank" rel="noreferrer">Code on GitHub</a>
        </div>
      </div>
      <div class="hero-demo">
        <p class="hero-demo-title">What you will build:</p>
        <HomeScene />
      </div>
    </div>
  </section>
</template>
