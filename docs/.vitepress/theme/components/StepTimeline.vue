<script setup lang="ts">
import { computed, ref } from 'vue'
import { withBase } from 'vitepress'
import { steps } from '../../steps'

const selected = ref(0)
const step = computed(() => steps[selected.value])
const part1 = steps.filter(s => s.part === 1)
const part2 = steps.filter(s => s.part === 2)
const indexOf = (n: string) => steps.findIndex(s => s.n === n)
</script>

<template>
  <div class="timeline">
    <div class="tl-route">
      <div class="tl-part part-1">
        <p class="tl-part-name">Part 1: Build the app</p>
        <ol class="tl-nodes">
          <li v-for="s in part1" :key="s.n">
            <a class="tl-node" :class="{ active: step.n === s.n }" :href="withBase(`/guide/${s.slug}`)"
               @mouseenter="selected = indexOf(s.n)" @focus="selected = indexOf(s.n)">
              <span class="tl-dot">{{ s.n }}</span>
              <span class="tl-short">{{ s.short }}</span>
            </a>
          </li>
        </ol>
      </div>
      <div class="tl-junction" aria-hidden="true"><span>Entra ID joins here</span></div>
      <div class="tl-part part-2">
        <p class="tl-part-name">Part 2: Add Entra ID</p>
        <ol class="tl-nodes" start="8">
          <li v-for="s in part2" :key="s.n">
            <a class="tl-node" :class="{ active: step.n === s.n }" :href="withBase(`/guide/${s.slug}`)"
               @mouseenter="selected = indexOf(s.n)" @focus="selected = indexOf(s.n)">
              <span class="tl-dot">{{ s.n }}</span>
              <span class="tl-short">{{ s.short }}</span>
            </a>
          </li>
        </ol>
      </div>
    </div>

    <div class="tl-detail" :class="`part-${step.part}`" aria-live="polite">
      <p class="tl-detail-num">{{ step.n }}</p>
      <div>
        <h3 class="tl-detail-title">{{ step.title }}</h3>
        <p class="tl-detail-text">{{ step.summary }}</p>
        <a class="tl-detail-link" :href="withBase(`/guide/${step.slug}`)">Open step {{ step.n }}</a>
      </div>
    </div>

    <ol class="tl-list">
      <li v-for="s in steps" :key="s.n" :class="`part-${s.part}`">
        <a :href="withBase(`/guide/${s.slug}`)">
          <span class="tl-list-num">{{ s.n }}</span>
          <span><strong>{{ s.title }}</strong><br />{{ s.summary }}</span>
        </a>
      </li>
    </ol>
  </div>
</template>
