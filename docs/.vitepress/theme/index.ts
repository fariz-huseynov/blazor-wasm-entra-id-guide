import { h } from 'vue'
import DefaultTheme from 'vitepress/theme'
import type { Theme } from 'vitepress'
import StepProgress from './components/StepProgress.vue'
import HomeHero from './components/HomeHero.vue'
import StepTimeline from './components/StepTimeline.vue'
import ArchDiagram from './components/ArchDiagram.vue'
import RolesMatrix from './components/RolesMatrix.vue'
import ImageLightbox from './components/ImageLightbox.vue'
import './custom.css'

export default {
  extends: DefaultTheme,
  Layout: () => h(DefaultTheme.Layout, null, {
    'doc-before': () => h(StepProgress),
    'layout-bottom': () => h(ImageLightbox),
  }),
  enhanceApp({ app }) {
    app.component('HomeHero', HomeHero)
    app.component('StepTimeline', StepTimeline)
    app.component('ArchDiagram', ArchDiagram)
    app.component('RolesMatrix', RolesMatrix)
  },
} satisfies Theme
