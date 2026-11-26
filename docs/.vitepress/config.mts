import { defineConfig } from 'vitepress'
import { steps, repo } from './steps'

const base = '/blazor-wasm-entra-id-guide/'

export default defineConfig({
  title: 'Blazor + Entra ID',
  description: 'Build a .NET 10 Blazor WebAssembly app with Microsoft Entra ID sign-in, app roles and a protected API, one commit at a time.',
  lang: 'en-US',
  base,
  cleanUrls: true,
  lastUpdated: true,
  srcExclude: ['**/node_modules/**'],
  // The guide links to the app running on your own machine.
  ignoreDeadLinks: [/^https?:\/\/localhost/],

  head: [
    ['link', { rel: 'icon', type: 'image/svg+xml', href: `${base}favicon.svg` }],
    ['link', { rel: 'preconnect', href: 'https://fonts.googleapis.com' }],
    ['link', { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' }],
    ['link', {
      rel: 'stylesheet',
      href: 'https://fonts.googleapis.com/css2?family=Archivo:wdth,wght@112..125,500..800&family=IBM+Plex+Mono:wght@400;500;600&family=IBM+Plex+Sans:ital,wght@0,400;0,500;0,600;1,400&display=swap',
    }],
    ['meta', { name: 'theme-color', content: '#0f3b6e' }],
    ['meta', { property: 'og:title', content: 'Blazor WebAssembly + Microsoft Entra ID, step by step' }],
    ['meta', { property: 'og:description', content: '12 commits from an empty folder to sign-in, app roles and a protected API.' }],
    ['meta', { property: 'og:image', content: `https://fariz-huseynov.github.io${base}og.png` }],
  ],

  markdown: {
    theme: { light: 'github-light', dark: 'github-dark' },
    config(md) {
      // The guide links to source files with paths like ../../src/... (they work on GitHub).
      // On the website those files don't exist, so point them at the file on GitHub instead.
      const render = md.renderer.rules.link_open ?? ((t, i, o, _e, s) => s.renderToken(t, i, o))
      md.renderer.rules.link_open = (tokens, idx, options, env, self) => {
        const href = tokens[idx].attrGet('href')
        if (href && href.startsWith('../../')) {
          tokens[idx].attrSet('href', `${repo}/blob/main/${href.replace(/^(\.\.\/)+/, '')}`)
          tokens[idx].attrSet('target', '_blank')
          tokens[idx].attrSet('rel', 'noreferrer')
        }
        return render(tokens, idx, options, env, self)
      }
    },
  },

  themeConfig: {
    logo: { src: '/logo.svg', alt: '' },
    siteTitle: 'Blazor + Entra ID',
    nav: [
      { text: 'Start the guide', link: '/guide/01-create-project' },
      { text: 'Testing', link: '/guide/testing-locally' },
      { text: 'Troubleshooting', link: '/guide/troubleshooting' },
    ],
    sidebar: {
      '/guide/': [
        {
          text: 'Part 1: Build the app',
          items: steps.filter(s => s.part === 1).map(s => ({ text: `${s.n}  ${s.title}`, link: `/guide/${s.slug}` })),
        },
        {
          text: 'Part 2: Add Entra ID',
          items: steps.filter(s => s.part === 2).map(s => ({ text: `${s.n}  ${s.title}`, link: `/guide/${s.slug}` })),
        },
        {
          text: 'Reference',
          items: [
            { text: 'Testing locally', link: '/guide/testing-locally' },
            { text: 'Troubleshooting', link: '/guide/troubleshooting' },
          ],
        },
      ],
    },
    socialLinks: [{ icon: 'github', link: repo }],
    search: { provider: 'local' },
    outline: { level: [2, 3], label: 'On this page' },
    editLink: { pattern: `${repo}/edit/main/docs/:path`, text: 'Suggest a change on GitHub' },
    docFooter: { prev: 'Previous step', next: 'Next step' },
    footer: {
      message: 'MIT licensed. Every step is a commit: <code>git checkout step-01</code>.',
      copyright: 'A step-by-step guide to Blazor WebAssembly and Microsoft Entra ID.',
    },
  },
})
