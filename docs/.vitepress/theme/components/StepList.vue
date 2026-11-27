<script setup lang="ts">
import { withBase } from 'vitepress'
import { steps } from '../../steps'

const parts = [
  { n: 1, name: 'Part 1: Build the app', note: 'A Blazor WebAssembly app with a todo list and two APIs. No sign-in yet.' },
  { n: 2, name: 'Part 2: Add Entra ID', note: 'Sign-in, app roles, and protecting both APIs, one concern per step.' },
]
</script>

<template>
  <div class="steplist">
    <section v-for="p in parts" :key="p.n" class="sl-part" :class="`part-${p.n}`">
      <h3 class="sl-name">{{ p.name }}</h3>
      <p class="sl-note">{{ p.note }}</p>
      <ol class="sl-items">
        <li v-for="s in steps.filter(x => x.part === p.n)" :key="s.n">
          <a :href="withBase(`/guide/${s.slug}`)">
            <span class="sl-num">{{ s.n }}</span>
            <span class="sl-text"><strong>{{ s.title }}</strong><span>{{ s.summary }}</span></span>
          </a>
        </li>
      </ol>
    </section>
  </div>
</template>
