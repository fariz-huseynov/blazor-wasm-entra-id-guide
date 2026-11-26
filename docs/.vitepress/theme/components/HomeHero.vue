<script setup lang="ts">
import { ref } from 'vue'
import { withBase } from 'vitepress'
import { repo } from '../../steps'
import demoGif from '../../../guide/images/todos-contributor.gif'

const command = 'git clone https://github.com/fariz-huseynov/blazor-wasm-entra-id-guide.git'
const copied = ref(false)
async function copy() {
  try {
    await navigator.clipboard.writeText(command)
    copied.value = true
    setTimeout(() => (copied.value = false), 1800)
  } catch { /* clipboard blocked: the command is still visible to select */ }
}
</script>

<template>
  <section class="hero blueprint">
    <div class="hero-inner">
      <div class="hero-copy">
        <h1 class="hero-title">
          <span>Blazor WebAssembly</span>
          <span class="hero-plus" aria-hidden="true">+</span>
          <span>Microsoft Entra ID,</span>
          <span class="hero-sub">one commit at a time</span>
        </h1>
        <p class="hero-lede">
          Twelve steps take a .NET&nbsp;10 app from an empty folder to Entra ID sign-in, app roles,
          and an API that other systems call with access tokens. Each step is a git commit you can check out and run.
        </p>
        <div class="hero-actions">
          <a class="btn btn-primary" :href="withBase('/guide/01-create-project')">Start with step 01</a>
          <a class="btn btn-ghost" :href="repo" target="_blank" rel="noreferrer">Browse the code on GitHub</a>
        </div>
        <div class="hero-cmd">
          <code>{{ command }}</code>
          <button type="button" class="hero-copy-btn" @click="copy" :aria-label="copied ? 'Copied' : 'Copy the clone command'">
            {{ copied ? 'Copied' : 'Copy' }}
          </button>
        </div>
      </div>

      <figure class="hero-sheet">
        <div class="sheet-frame">
          <span class="reg reg-tl" aria-hidden="true"></span>
          <span class="reg reg-tr" aria-hidden="true"></span>
          <span class="reg reg-bl" aria-hidden="true"></span>
          <span class="reg reg-br" aria-hidden="true"></span>
          <img :src="demoGif" width="960" height="506"
               alt="The finished app: a signed-in Contributor adds a todo, ticks it and deletes it." />
        </div>
        <figcaption>
          <table class="title-block">
            <tbody>
              <tr><th scope="row">Drawing</th><td colspan="3">Todo app, signed in as a Contributor</td></tr>
              <tr><th scope="row">Stack</th><td colspan="3">.NET 10, Blazor WebAssembly, MudBlazor, Microsoft Entra ID</td></tr>
              <tr><th scope="row">Sheets</th><td>12 steps in 2 parts</td><th scope="row">Licence</th><td>MIT</td></tr>
            </tbody>
          </table>
        </figcaption>
      </figure>
    </div>
  </section>
</template>
