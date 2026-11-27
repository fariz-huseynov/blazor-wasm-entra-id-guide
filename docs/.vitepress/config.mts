import { defineConfig } from 'vitepress'
import { steps, repo } from './steps'

const base = '/blazor-wasm-entra-id-guide/'
const site = `https://fariz-huseynov.github.io${base}`
const siteName = 'Blazor WebAssembly + Microsoft Entra ID guide'

// Search-friendly title and description for every page.
const homeTitle = 'Blazor WebAssembly + Microsoft Entra ID: a step-by-step .NET 10 tutorial'
const homeDescription = 'Build a .NET 10 Blazor WebAssembly app secured with Microsoft Entra ID (Azure AD) in 12 steps: sign-in, app roles, a protected API with bearer tokens, Scalar and DefaultAzureCredential. Each step is a git commit.'
const pageMeta: Record<string, { title?: string, description: string }> = {
  'guide/testing-locally.md': { description: 'Test the Blazor WebAssembly and Microsoft Entra ID app locally: browser sign-in, app roles, Scalar with a bearer token, the console client, and who can do what.' },
  'guide/troubleshooting.md': { description: 'Fix common Microsoft Entra ID errors in a Blazor WebAssembly app: AADSTS50011, AADSTS65001, AADSTS500011, a missing roles claim, 401 vs 403, and HTTPS certificates.' },
}
const seoTitles: Record<string, string> = {
  '01': 'Create a Blazor WebAssembly project in .NET 10',
  '02': 'Add a shared class library to a Blazor Web App',
  '03': 'Add MudBlazor to a Blazor WebAssembly app',
  '04': 'Show the Blazor render mode: prerendering vs WebAssembly',
  '05': 'A todo list with prerendering and PersistentState in Blazor',
  '06': 'Add a versioned external API with ASP.NET Core minimal APIs',
  '07': 'Document an ASP.NET Core API with OpenAPI and Scalar',
  '08': 'Sign in with Microsoft Entra ID in Blazor WebAssembly',
  '09': 'Use Microsoft Entra ID app roles in Blazor',
  '10': 'Protect a Blazor UI API with cookies, roles and antiforgery',
  '11': 'Protect an ASP.NET Core API with Entra ID bearer tokens',
  '12': 'Call a protected API with DefaultAzureCredential',
}
for (const st of steps) {
  pageMeta[`guide/${st.slug}.md`] = {
    title: `Step ${st.n}: ${seoTitles[st.n]}`,
    description: `${st.summary} Step ${st.n} of 12 in a .NET 10 Blazor WebAssembly and Microsoft Entra ID tutorial.`,
  }
}
const urlFor = (relativePath: string) => site + relativePath.replace(/(^|\/)index\.md$/, '$1').replace(/\.md$/, '')

export default defineConfig({
  title: 'Blazor + Entra ID',
  titleTemplate: ':title | Blazor WebAssembly + Entra ID guide',
  description: homeDescription,
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
    ['meta', { name: 'author', content: 'fariz-huseynov' }],
    ['meta', { property: 'og:site_name', content: siteName }],
    ['meta', { property: 'og:locale', content: 'en_US' }],
    ['meta', { property: 'og:image', content: `${site}og.png` }],
    ['meta', { property: 'og:image:width', content: '1200' }],
    ['meta', { property: 'og:image:height', content: '630' }],
    ['meta', { name: 'twitter:card', content: 'summary_large_image' }],
    ['meta', { name: 'twitter:image', content: `${site}og.png` }],
  ],

  sitemap: { hostname: site },

  // Per-page title and description (used by search results and link previews).
  transformPageData(pageData) {
    const meta = pageMeta[pageData.relativePath]
    if (pageData.relativePath === 'index.md') {
      pageData.title = homeTitle
      pageData.titleTemplate = false
      pageData.description = homeDescription
    } else if (meta) {
      if (meta.title) pageData.title = meta.title
      pageData.description = meta.description
    }
  },

  // Canonical URL, Open Graph / Twitter tags and structured data for each page.
  transformHead({ pageData }) {
    const url = urlFor(pageData.relativePath)
    const isHome = pageData.relativePath === 'index.md'
    const title = isHome ? homeTitle : `${pageData.title} | ${siteName}`
    const description = pageData.description || homeDescription
    const data = isHome
      ? {
          '@context': 'https://schema.org',
          '@graph': [
            { '@type': 'WebSite', name: siteName, url: site, description: homeDescription, inLanguage: 'en' },
            {
              '@type': 'ItemList',
              name: 'Steps',
              itemListElement: steps.map((st, i) => ({ '@type': 'ListItem', position: i + 1, name: `Step ${st.n}: ${st.title}`, url: `${site}guide/${st.slug}` })),
            },
          ],
        }
      : {
          '@context': 'https://schema.org',
          '@graph': [
            {
              '@type': 'TechArticle',
              headline: pageData.title,
              description,
              url,
              inLanguage: 'en',
              proficiencyLevel: 'Beginner',
              dependencies: '.NET 10 SDK',
              isPartOf: { '@type': 'WebSite', name: siteName, url: site },
              author: { '@type': 'Person', name: 'fariz-huseynov', url: 'https://github.com/fariz-huseynov' },
              ...(pageData.lastUpdated ? { dateModified: new Date(pageData.lastUpdated).toISOString() } : {}),
            },
            {
              '@type': 'BreadcrumbList',
              itemListElement: [
                { '@type': 'ListItem', position: 1, name: 'Guide', item: site },
                { '@type': 'ListItem', position: 2, name: pageData.title, item: url },
              ],
            },
          ],
        }
    return [
      ['link', { rel: 'canonical', href: url }],
      ['meta', { property: 'og:type', content: isHome ? 'website' : 'article' }],
      ['meta', { property: 'og:url', content: url }],
      ['meta', { property: 'og:title', content: title }],
      ['meta', { property: 'og:description', content: description }],
      ['meta', { name: 'twitter:title', content: title }],
      ['meta', { name: 'twitter:description', content: description }],
      ['script', { type: 'application/ld+json' }, JSON.stringify(data)],
    ]
  },

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
