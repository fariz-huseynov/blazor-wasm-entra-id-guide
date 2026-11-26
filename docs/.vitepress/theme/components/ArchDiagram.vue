<script setup lang="ts">
// "What you end up with": the architecture as a small simulator.
// Pick a scenario, press Play (or step through it), and watch each request travel between the pieces,
// with a plain-language explanation and the HTTP log line for every hop.
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

type NodeKey = 'browser' | 'other' | 'ui' | 'v1' | 'store' | 'entra' | 'server'
type Kind = 'cookie' | 'token' | 'ok' | 'deny' | 'plain'

// Routes between the pieces. "a" and "b" are the nodes at the start and end of the path.
const routes: Record<string, { d: string, a: NodeKey, b: NodeKey }> = {
  browserUi: { d: 'M250 130 H370', a: 'browser', b: 'ui' },
  browserServer: { d: 'M250 64 H350', a: 'browser', b: 'server' },
  otherV1: { d: 'M250 360 H370', a: 'other', b: 'v1' },
  uiStore: { d: 'M500 204 V224', a: 'ui', b: 'store' },
  v1Store: { d: 'M500 300 V278', a: 'v1', b: 'store' },
  browserEntra: { d: 'M135 40 V8 H845 V120', a: 'browser', b: 'entra' },
  otherEntra: { d: 'M135 420 V470 H845 V340', a: 'other', b: 'entra' },
}

interface Hop {
  route: keyof typeof routes
  back?: boolean          // travel from b to a
  label: string           // text on the moving packet
  kind: Kind
  verdict?: 'ok' | 'deny' // marker shown where the packet arrives
  title: string
  text: string
  log: string
  signedIn?: boolean      // browser state after this hop
}

interface Scenario { name: string, who: string, startSignedIn: boolean, hops: Hop[] }

const scenarios: Scenario[] = [
  {
    name: 'Alex signs in', who: 'Browser', startSignedIn: false,
    hops: [
      { route: 'browserServer', label: 'GET /authentication/login', kind: 'plain',
        title: 'Alex clicks Sign in', text: 'The browser asks the server to sign in. The server answers with a redirect to Microsoft Entra ID.',
        log: 'GET  /authentication/login            → 302 to Entra ID' },
      { route: 'browserEntra', label: 'sign-in page', kind: 'plain',
        title: 'Microsoft asks for the password', text: 'Alex signs in on Microsoft\'s own page, with MFA if the tenant needs it. The app never sees the password.',
        log: 'GET  login.microsoftonline.com/…/authorize  response_type=id_token' },
      { route: 'browserEntra', back: true, label: 'ID token', kind: 'token',
        title: 'Entra ID returns an ID token', text: 'The ID token says who Alex is and lists the app roles assigned to Alex: Reader and Contributor.',
        log: '200  form post to /signin-oidc          id_token (roles: Reader, Contributor)' },
      { route: 'browserServer', label: 'POST /signin-oidc', kind: 'token', verdict: 'ok',
        title: 'The server checks the token', text: 'The server checks the signature, the audience and the nonce. No client secret is needed, because the token arrives directly.',
        log: 'POST /signin-oidc                       signature ✓  audience ✓  nonce ✓' },
      { route: 'browserServer', back: true, label: 'cookie', kind: 'cookie', signedIn: true,
        title: 'Alex is signed in', text: 'The server keeps Alex in an encrypted, HttpOnly cookie. WebAssembly gets Alex\'s name and roles, never a token.',
        log: '302  Set-Cookie: .AspNetCore.Cookies (HttpOnly)  → back to the app' },
    ],
  },
  {
    name: 'A Contributor adds a todo', who: 'Browser', startSignedIn: true,
    hops: [
      { route: 'browserUi', label: 'POST + cookie', kind: 'cookie', verdict: 'ok',
        title: 'The Todos page sends the new todo', text: 'The request carries the sign-in cookie and an antiforgery token. The UI API checks both, then the role.',
        log: 'POST /api/ui/todos   cookie ✓  antiforgery ✓  role Contributor ✓' },
      { route: 'uiStore', label: 'save', kind: 'plain',
        title: 'The todo is saved', text: 'The endpoint calls the same ITodoService that the server uses for prerendering.',
        log: '     InMemoryTodoService.CreateAsync("Write the guide")' },
      { route: 'browserUi', back: true, label: '201 Created', kind: 'ok',
        title: 'The list updates', text: 'The page adds the new todo and shows "Todo added".',
        log: '201  Created  Location: /api/ui/todos/…' },
    ],
  },
  {
    name: 'A Reader tries to add one', who: 'Browser', startSignedIn: true,
    hops: [
      { route: 'browserUi', label: 'POST + cookie', kind: 'cookie', verdict: 'deny',
        title: 'Signed in, but not allowed', text: 'Readers don\'t see the Add button, but someone could still send the request by hand. The API checks the role anyway.',
        log: 'POST /api/ui/todos   cookie ✓  antiforgery ✓  role Contributor ✕' },
      { route: 'browserUi', back: true, label: '403 Forbidden', kind: 'deny',
        title: 'The API says no', text: '403 means "I know who you are, but you may not do this". Nothing is saved.',
        log: '403  Forbidden' },
    ],
  },
  {
    name: 'The console app reads todos', who: 'Other systems', startSignedIn: false,
    hops: [
      { route: 'otherEntra', label: 'get token', kind: 'plain',
        title: 'The console app asks for a token', text: 'DefaultAzureCredential uses your az login on your machine, or a managed identity or service principal elsewhere.',
        log: 'DefaultAzureCredential → AzureCliCredential  scope api://<client-id>/.default' },
      { route: 'otherEntra', back: true, label: 'access token', kind: 'token',
        title: 'Entra ID issues an access token', text: 'The token is for this API only. It carries your roles and, because it is a user token, the scope access_as_user.',
        log: '200  access_token  aud=api://<client-id>  scp=access_as_user  roles=[Reader, Api.Writer]' },
      { route: 'otherV1', label: 'GET + Bearer', kind: 'token', verdict: 'ok',
        title: 'The external API checks the token', text: '/api/v1 only accepts bearer tokens. It checks signature, audience, scope and role.',
        log: 'GET  /api/v1/todos   bearer ✓  audience ✓  scope ✓  role Reader ✓' },
      { route: 'v1Store', label: 'read', kind: 'plain',
        title: 'Same data, different door', text: 'Both APIs use the same todo store. Only the way callers prove who they are is different.',
        log: '     InMemoryTodoService.GetAllAsync()' },
      { route: 'otherV1', back: true, label: '200 OK', kind: 'ok',
        title: 'The todos come back', text: 'The console app prints them, then tries a POST, which needs the Api.Writer role.',
        log: '200  OK  [3 todos]' },
    ],
  },
  {
    name: 'A call without a token', who: 'Other systems', startSignedIn: false,
    hops: [
      { route: 'otherV1', label: 'GET, no token', kind: 'plain', verdict: 'deny',
        title: 'No token, no entry', text: '/api/v1 ignores cookies and anonymous calls. Only an Entra ID access token gets in.',
        log: 'GET  /api/v1/todos   Authorization header missing' },
      { route: 'otherV1', back: true, label: '401', kind: 'deny',
        title: '401 Unauthorized', text: '401 means "I don\'t know who you are". The answer tells the caller to come back with a bearer token.',
        log: '401  Unauthorized  WWW-Authenticate: Bearer' },
    ],
  },
]

const sIdx = ref(0)
const step = ref(-1)          // index of the hop that is moving or done; -1 = not started
const t = ref(1)              // progress of the current hop, 0..1
const playing = ref(false)
const pathEls: Record<string, SVGPathElement | null> = {}
const reduceMotion = ref(false)

const scenario = computed(() => scenarios[sIdx.value])
const hop = computed(() => (step.value >= 0 ? scenario.value.hops[step.value] : null))
const logs = computed(() => scenario.value.hops.slice(0, step.value + 1).map(h => h.log))
const signedIn = computed(() => {
  let s = scenario.value.startSignedIn
  for (let i = 0; i <= step.value; i++) if (scenario.value.hops[i].signedIn) s = true
  return s
})
const finished = computed(() => step.value === scenario.value.hops.length - 1 && t.value >= 1)

const lit = computed<Set<NodeKey>>(() => {
  const h = hop.value
  if (!h) return new Set()
  const r = routes[h.route]
  const set = new Set<NodeKey>([r.a, r.b])
  if (r.a === 'ui' || r.b === 'ui' || r.a === 'v1' || r.b === 'v1') set.add('server')
  return set
})

const packet = computed(() => {
  const h = hop.value
  const el = h ? pathEls[h.route] : null
  if (!h || !el) return null
  const len = el.getTotalLength()
  const eased = t.value < 0.5 ? 2 * t.value * t.value : 1 - Math.pow(-2 * t.value + 2, 2) / 2
  const p = el.getPointAtLength((h.back ? 1 - eased : eased) * len)
  const w = Math.round(h.label.length * 7.3 + 22)
  return { x: p.x, y: p.y, w, label: h.label, kind: h.kind }
})

const marker = computed(() => {
  const h = hop.value
  const el = h ? pathEls[h.route] : null
  if (!h?.verdict || !el || t.value < 1) return null
  const p = el.getPointAtLength(h.back ? 0 : el.getTotalLength())
  return { x: p.x, y: p.y, verdict: h.verdict }
})

let raf = 0
let timer: ReturnType<typeof setTimeout> | undefined
function stopTimers() { cancelAnimationFrame(raf); clearTimeout(timer) }

function animateHop(onDone: () => void) {
  const h = hop.value
  const el = h ? pathEls[h.route] : null
  const dur = reduceMotion.value ? 0 : Math.min(1800, Math.max(700, (el?.getTotalLength() ?? 200) * 1.6))
  if (dur === 0) { t.value = 1; onDone(); return }
  const start = performance.now()
  t.value = 0
  const frame = (now: number) => {
    t.value = Math.min(1, (now - start) / dur)
    if (t.value < 1) raf = requestAnimationFrame(frame)
    else onDone()
  }
  raf = requestAnimationFrame(frame)
}

function advance() {
  if (step.value >= scenario.value.hops.length - 1) { playing.value = false; return }
  step.value++
  animateHop(() => {
    if (step.value >= scenario.value.hops.length - 1) { playing.value = false; return }
    if (!playing.value) return
    const pause = hop.value?.verdict ? 1500 : 1100
    timer = setTimeout(advance, reduceMotion.value ? 2200 : pause)
  })
}

function play() {
  stopTimers()
  if (finished.value) step.value = -1
  playing.value = true
  advance()
}
function pause() { playing.value = false; stopTimers(); t.value = 1 }
function next() { pause(); if (step.value < scenario.value.hops.length - 1) { step.value++; animateHop(() => {}) } }
function prev() { pause(); if (step.value > -1) { step.value--; t.value = 1 } }
function choose(i: number) { pause(); sIdx.value = i; step.value = -1; t.value = 1 }

function setPath(key: string) {
  return (el: unknown) => { pathEls[key] = el as SVGPathElement | null }
}

onMounted(() => {
  reduceMotion.value = window.matchMedia('(prefers-reduced-motion: reduce)').matches
})
onBeforeUnmount(stopTimers)
</script>

<template>
  <figure class="arch sim">
    <div class="sim-tabs" role="tablist" aria-label="Scenarios">
      <button v-for="(s, i) in scenarios" :key="s.name" type="button" role="tab"
              class="sim-tab" :aria-selected="i === sIdx" @click="choose(i)">
        {{ s.name }}
      </button>
    </div>

    <svg viewBox="0 0 960 500" role="img" aria-labelledby="arch-title arch-desc">
      <title id="arch-title">How the pieces fit together</title>
      <desc id="arch-desc">
        The browser runs the Blazor WebAssembly app and calls /api/ui/todos with the sign-in cookie.
        Other systems, such as the console app, call /api/v1/todos with an Entra ID access token.
        Both APIs live in the same ASP.NET Core server. Microsoft Entra ID signs users in and issues tokens.
      </desc>
      <defs>
        <marker id="arr" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
          <path d="M0 0 L10 5 L0 10 z" class="a-head" />
        </marker>
      </defs>

      <!-- Routes (visible lines) -->
      <path :ref="setPath('browserUi')" :d="routes.browserUi.d" class="a-line part-1" marker-end="url(#arr)" />
      <path :ref="setPath('otherV1')" :d="routes.otherV1.d" class="a-line part-2" marker-end="url(#arr)" />
      <path :ref="setPath('browserServer')" :d="routes.browserServer.d" class="a-line thin dash" marker-end="url(#arr)" />
      <path :ref="setPath('uiStore')" :d="routes.uiStore.d" class="a-line thin" />
      <path :ref="setPath('v1Store')" :d="routes.v1Store.d" class="a-line thin" />
      <path :ref="setPath('browserEntra')" :d="routes.browserEntra.d" class="a-line dash" marker-end="url(#arr)" />
      <path :ref="setPath('otherEntra')" :d="routes.otherEntra.d" class="a-line dash" marker-end="url(#arr)" />
      <text x="262" y="120" class="a-label">cookie</text>
      <text x="262" y="56" class="a-label">sign-in</text>
      <text x="262" y="350" class="a-label">token</text>
      <text x="490" y="28" class="a-label a-label-bg" text-anchor="middle">sign in on Microsoft's page</text>
      <text x="490" y="462" class="a-label" text-anchor="middle">get an access token (az login, managed identity, service principal)</text>

      <!-- Pieces -->
      <g class="a-box a-client" :class="{ lit: lit.has('browser') }">
        <rect x="20" y="40" width="230" height="130" rx="10" />
        <text x="40" y="76" class="a-h">Browser</text>
        <text x="40" y="102" class="a-t">Blazor WebAssembly app</text>
        <g class="a-state" :class="{ on: signedIn && scenario.who === 'Browser' }">
          <rect x="40" y="122" width="190" height="28" rx="14" />
          <text x="135" y="141" text-anchor="middle">
            {{ scenario.who !== 'Browser' ? 'Idle' : signedIn ? (sIdx === 2 ? 'Signed in: Reader' : 'Signed in: Contributor') : 'Not signed in' }}
          </text>
        </g>
      </g>
      <g class="a-box a-client" :class="{ lit: lit.has('other') }">
        <rect x="20" y="290" width="230" height="130" rx="10" />
        <text x="40" y="326" class="a-h">Other systems</text>
        <text x="40" y="352" class="a-t">Console app, scripts,</text>
        <text x="40" y="372" class="a-t">services. Tokens from</text>
        <text x="40" y="392" class="a-t">DefaultAzureCredential</text>
      </g>
      <g class="a-box a-server" :class="{ lit: lit.has('server') }">
        <rect x="350" y="44" width="300" height="396" rx="12" />
        <text x="370" y="74" class="a-h">ASP.NET Core server</text>
        <text x="370" y="94" class="a-t">BlazorWebApplication.Server</text>
      </g>
      <g class="a-port part-1" :class="{ lit: lit.has('ui') }">
        <rect x="370" y="104" width="260" height="100" rx="8" />
        <text x="388" y="134" class="a-code">/api/ui/todos</text>
        <text x="388" y="158" class="a-t">Sign-in cookie only</text>
        <text x="388" y="180" class="a-t">Antiforgery check on changes</text>
      </g>
      <g class="a-port part-2" :class="{ lit: lit.has('v1') }">
        <rect x="370" y="300" width="260" height="104" rx="8" />
        <text x="388" y="332" class="a-code">/api/v1/todos</text>
        <text x="388" y="356" class="a-t">Bearer token only</text>
        <text x="388" y="378" class="a-t">Documented in Scalar</text>
      </g>
      <g class="a-store" :class="{ lit: lit.has('store') }">
        <rect x="430" y="224" width="140" height="54" rx="27" />
        <text x="500" y="256" class="a-t" text-anchor="middle">Todo store</text>
      </g>
      <g class="a-box a-entra" :class="{ lit: lit.has('entra') }">
        <rect x="750" y="120" width="190" height="220" rx="10" />
        <text x="770" y="156" class="a-h">Microsoft</text>
        <text x="770" y="180" class="a-h">Entra ID</text>
        <text x="770" y="212" class="a-t">One app registration</text>
        <text x="770" y="234" class="a-t">Roles: Reader,</text>
        <text x="770" y="256" class="a-t">Contributor, Api.Writer</text>
        <text x="770" y="278" class="a-t">Scope: access_as_user</text>
      </g>

      <!-- Moving request -->
      <g v-if="packet" class="packet" :class="`k-${packet.kind}`" :transform="`translate(${packet.x} ${packet.y})`">
        <rect :x="-packet.w / 2" y="-13" :width="packet.w" height="26" rx="13" />
        <text y="4.5" text-anchor="middle">{{ packet.label }}</text>
      </g>
      <g v-if="marker" class="verdict" :class="marker.verdict" :transform="`translate(${marker.x} ${marker.y - 30})`">
        <circle r="14" />
        <text y="5" text-anchor="middle">{{ marker.verdict === 'ok' ? '✓' : '✕' }}</text>
      </g>
    </svg>

    <div class="sim-panel">
      <div class="sim-story" aria-live="polite">
        <p class="sim-count">
          <template v-if="step >= 0">Step {{ step + 1 }} of {{ scenario.hops.length }}</template>
          <template v-else>{{ scenario.hops.length }} steps</template>
        </p>
        <h3 class="sim-title">{{ hop ? hop.title : scenario.name }}</h3>
        <p class="sim-text">
          {{ hop ? hop.text : 'Press Play to watch the requests travel, or use the arrows to go one step at a time.' }}
        </p>
        <div class="sim-controls">
          <button type="button" class="sim-btn" @click="prev" :disabled="step < 0" aria-label="Previous step">‹</button>
          <button v-if="!playing" type="button" class="sim-btn sim-play" @click="play">
            {{ finished ? 'Replay' : step >= 0 ? 'Continue' : 'Play' }}
          </button>
          <button v-else type="button" class="sim-btn sim-play" @click="pause">Pause</button>
          <button type="button" class="sim-btn" @click="next" :disabled="step >= scenario.hops.length - 1" aria-label="Next step">›</button>
        </div>
      </div>
      <div class="sim-log" role="log" aria-label="Request log">
        <p class="sim-log-head">Request log</p>
        <ol>
          <li v-for="(l, i) in logs" :key="i" :class="{ latest: i === logs.length - 1 }">{{ l }}</li>
        </ol>
        <p v-if="!logs.length" class="sim-log-empty">Nothing sent yet.</p>
      </div>
    </div>
  </figure>
</template>
