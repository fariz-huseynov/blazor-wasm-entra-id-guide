<script setup lang="ts">
// Click (or press Enter on) any screenshot or GIF to open it in a full-screen viewer.
// In the viewer: click the image or press Z to switch between "fit to screen" and "actual size"
// (drag or scroll to move around), use the arrow keys or buttons for the other images on the page,
// and Esc to close.
import { nextTick, onMounted, onBeforeUnmount, ref, computed, watch } from 'vue'
import { useRoute } from 'vitepress'

const SELECTOR = '.vp-doc img, .hero-sheet img'

const dialog = ref<HTMLDialogElement | null>(null)
const stage = ref<HTMLDivElement | null>(null)
const images = ref<HTMLImageElement[]>([])
const index = ref(0)
const actual = ref(false)
const current = computed(() => images.value[index.value])
const canZoom = ref(true)

function collect() {
  images.value = Array.from(document.querySelectorAll<HTMLImageElement>(SELECTOR))
  for (const img of images.value) {
    if (img.dataset.zoomable) continue
    img.dataset.zoomable = 'true'
    img.tabIndex = 0
    img.setAttribute('role', 'button')
    img.setAttribute('aria-label', `Open larger view: ${img.alt || 'image'}`)
  }
}

function open(img: HTMLImageElement) {
  collect()
  index.value = Math.max(0, images.value.indexOf(img))
  actual.value = false
  dialog.value?.showModal()
  document.documentElement.classList.add('lb-open')
  updateCanZoom()
}

function close() {
  dialog.value?.close()
}

function onClosed() {
  document.documentElement.classList.remove('lb-open')
  current.value?.focus({ preventScroll: true })
}

function go(step: number) {
  if (images.value.length < 2) return
  index.value = (index.value + step + images.value.length) % images.value.length
  actual.value = false
  nextTick(updateCanZoom)
}

function updateCanZoom() {
  const img = current.value
  if (!img) return
  // Only offer "actual size" when the image is bigger than the space it gets when fitted.
  const w = img.naturalWidth, h = img.naturalHeight
  canZoom.value = w > window.innerWidth * 0.94 || h > window.innerHeight * 0.82
}

async function toggleZoom(e?: MouseEvent) {
  if (!canZoom.value) return
  const box = stage.value
  const imgEl = box?.querySelector('img')
  // Remember where the user clicked, so actual size opens at that spot.
  let fx = 0.5, fy = 0.5
  if (e && imgEl) {
    const r = imgEl.getBoundingClientRect()
    fx = (e.clientX - r.left) / r.width
    fy = (e.clientY - r.top) / r.height
  }
  actual.value = !actual.value
  await nextTick()
  if (actual.value && box && current.value) {
    box.scrollLeft = current.value.naturalWidth * fx - box.clientWidth / 2
    box.scrollTop = current.value.naturalHeight * fy - box.clientHeight / 2
  }
}

// Drag to pan in actual size.
let drag: { x: number, y: number, l: number, t: number, moved: boolean } | null = null
function down(e: PointerEvent) {
  if (!actual.value || !stage.value || e.pointerType === 'touch') return
  drag = { x: e.clientX, y: e.clientY, l: stage.value.scrollLeft, t: stage.value.scrollTop, moved: false }
}
function move(e: PointerEvent) {
  if (!drag || !stage.value) return
  const dx = e.clientX - drag.x, dy = e.clientY - drag.y
  if (Math.abs(dx) + Math.abs(dy) > 4) drag.moved = true
  stage.value.scrollLeft = drag.l - dx
  stage.value.scrollTop = drag.t - dy
}
function up() { setTimeout(() => (drag = null)) }
function imageClick(e: MouseEvent) {
  if (drag?.moved) return
  toggleZoom(e)
}

function onDocClick(e: MouseEvent) {
  const img = (e.target as HTMLElement).closest?.(SELECTOR) as HTMLImageElement | null
  if (img && !img.closest('a')) { e.preventDefault(); open(img) }
}
function onDocKey(e: KeyboardEvent) {
  const t = e.target as HTMLElement
  if ((e.key === 'Enter' || e.key === ' ') && t.matches?.(SELECTOR)) { e.preventDefault(); open(t as HTMLImageElement) }
}
function onDialogKey(e: KeyboardEvent) {
  if (e.key === 'ArrowRight') { e.preventDefault(); go(1) }
  else if (e.key === 'ArrowLeft') { e.preventDefault(); go(-1) }
  else if (e.key.toLowerCase() === 'z') { e.preventDefault(); toggleZoom() }
}
function onBackdrop(e: MouseEvent) {
  if (e.target === dialog.value || e.target === stage.value) close()
}

const route = useRoute()
watch(() => route.path, () => nextTick(() => setTimeout(collect, 50)))
onMounted(() => {
  collect()
  document.addEventListener('click', onDocClick)
  document.addEventListener('keydown', onDocKey)
  window.addEventListener('resize', updateCanZoom)
})
onBeforeUnmount(() => {
  document.removeEventListener('click', onDocClick)
  document.removeEventListener('keydown', onDocKey)
  window.removeEventListener('resize', updateCanZoom)
})
</script>

<template>
  <dialog ref="dialog" class="lightbox" :class="{ actual }" aria-label="Image viewer"
          @close="onClosed" @keydown="onDialogKey" @click="onBackdrop">
    <div class="lb-bar">
      <p class="lb-caption">{{ current?.alt }}</p>
      <p v-if="images.length > 1" class="lb-count">{{ index + 1 }} / {{ images.length }}</p>
      <button v-if="canZoom" type="button" class="lb-btn" @click="toggleZoom()" :aria-pressed="actual">
        {{ actual ? 'Fit to screen' : 'Actual size' }}
      </button>
      <a v-if="current" class="lb-btn" :href="current.src" target="_blank" rel="noreferrer">Open file</a>
      <button type="button" class="lb-btn lb-close" @click="close" aria-label="Close (Esc)">Close</button>
    </div>
    <div ref="stage" class="lb-stage" @pointerdown="down" @pointermove="move" @pointerup="up" @pointerleave="up">
      <img v-if="current" :key="current.src" :src="current.src" :alt="current.alt"
           :class="{ zoomable: canZoom }" draggable="false" @click.stop="imageClick" @load="updateCanZoom" />
    </div>
    <template v-if="images.length > 1">
      <button type="button" class="lb-nav lb-prev" @click="go(-1)" aria-label="Previous image">‹</button>
      <button type="button" class="lb-nav lb-next" @click="go(1)" aria-label="Next image">›</button>
    </template>
  </dialog>
</template>
