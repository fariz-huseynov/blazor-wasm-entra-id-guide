<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, withBase } from 'vitepress'
import { steps } from '../../steps'

const route = useRoute()
const index = computed(() => steps.findIndex(s => route.path.replace(/\.html$/, '').endsWith(`/guide/${s.slug}`)))
const current = computed(() => (index.value >= 0 ? steps[index.value] : null))
</script>

<template>
  <nav v-if="current" class="step-progress" aria-label="Progress through the guide">
    <p class="sp-label">
      <span class="sp-num">Step {{ current.n }}</span> of 12
      <span class="sp-part" :class="`part-${current.part}`">
        {{ current.part === 1 ? 'Part 1: Build the app' : 'Part 2: Add Entra ID' }}
      </span>
    </p>
    <ol class="sp-track">
      <li v-for="(s, i) in steps" :key="s.n" :class="[`part-${s.part}`, { done: i < index, here: i === index }]">
        <a :href="withBase(`/guide/${s.slug}`)" :aria-current="i === index ? 'step' : undefined" :title="`Step ${s.n}: ${s.title}`">
          <span class="visually-hidden">Step {{ s.n }}: {{ s.title }}</span>
        </a>
      </li>
    </ol>
  </nav>
</template>
