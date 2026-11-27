<script setup lang="ts">
// Self-playing walkthrough for the home page: a browser signs in with Entra ID, adds a todo,
// then a console app calls the external API with an access token.
// Pauses when off screen or in a hidden tab; shows one still frame for reduced motion.
// @ts-nocheck
import { onBeforeUnmount, onMounted, ref } from 'vue'

const root = ref<HTMLElement | null>(null)
let stopAll = () => {}

onMounted(() => {
let ro: ResizeObserver | undefined, io: IntersectionObserver | undefined

const $ = id => root.value!.querySelector('#sc-' + id) as any;
const NS = 'http://www.w3.org/2000/svg';
const stage = $('stage'), vp = $('vp');
function fit() { const s = vp.clientWidth / 1000; stage.style.transform = `scale(${s})`; vp.style.height = (466 * s) + 'px'; }
ro = new ResizeObserver(fit); ro.observe(vp); fit();
const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;

// Time only runs while the band is on screen and the tab is visible.
let visible = false, gen = 0;
class Stop extends Error {}
const running = () => visible && !document.hidden;
function sleep(ms) {
  const my = gen;
  return new Promise((res, rej) => {
    let left = ms;
    const t = setInterval(() => {
      if (my !== gen) { clearInterval(t); rej(new Stop()); return; }
      if (running()) { left -= 40; if (left <= 0) { clearInterval(t); res(); } }
    }, 40);
  });
}

const COLORS = { plain: '#eaf2ff', cookie: '#9b87ff', token: '#5fb0ff', ok: '#5fe0a0' };
const TRAIL = { plain: 'white', cookie: 'violet', token: 'blue', ok: 'green' };
const ICON = { cookie: '●', token: '⚷', ok: '✓' };
function el(tag: string, attrs: Record<string, any>, text?: string) { const e = document.createElementNS(NS, tag); for (const k in attrs) e.setAttribute(k, attrs[k]); if (text) e.textContent = text; return e; }
function drawPacket(kind, label) {
  const g = $('pk'); g.innerHTML = '';
  const plain = kind === 'plain', c = COLORS[kind];
  const w = label.length * 7.4 + (plain ? 24 : 40);
  g.appendChild(el('rect', { x: -w / 2, y: -14, width: w, height: 28, rx: kind === 'token' ? 6 : 14, fill: plain ? '#0a2c55' : c, stroke: c, 'stroke-width': 2, class: 'glow', style: `color:${c}` }));
  if (!plain) g.appendChild(el('text', { x: -w / 2 + 12, y: 4.5, fill: '#08203f', 'font-size': 13 }, ICON[kind]));
  g.appendChild(el('text', { x: plain ? 0 : 9, y: 4.5, 'text-anchor': 'middle', fill: plain ? '#eaf2ff' : '#08203f' }, label));
}
// Move a request along a route, drawing a glowing trail behind it.
async function send(route, kind, label, back = false) {
  const path = $(route), len = path.getTotalLength();
  const trail = el('path', { d: path.getAttribute('d'), class: 'trail ' + TRAIL[kind] });
  trail.style.strokeDasharray = len; trail.style.strokeDashoffset = back ? -len : len;
  $('trails').appendChild(trail);
  drawPacket(kind, label);
  const g = $('pk'); g.setAttribute('opacity', 1);
  const dur = Math.max(1100, len * 3.2), my = gen;
  await new Promise((res, rej) => {
    let acc = 0, last = null;
    const frame = now => {
      if (my !== gen) { rej(new Stop()); return; }
      if (last !== null && running()) acc += now - last;
      last = now;
      const p = Math.min(1, acc / dur), e = p < .5 ? 2 * p * p : 1 - Math.pow(-2 * p + 2, 2) / 2;
      const at = path.getPointAtLength((back ? 1 - e : e) * len);
      g.setAttribute('transform', `translate(${at.x} ${at.y})`);
      trail.style.strokeDashoffset = (back ? -1 : 1) * len * (1 - e);
      if (p < 1) requestAnimationFrame(frame); else res();
    };
    requestAnimationFrame(frame);
  });
  g.setAttribute('opacity', 0);
  setTimeout(() => { trail.style.opacity = 0; setTimeout(() => trail.remove(), 900); }, 400);
}
async function cursorTo(x, y) { const c = $('cursor'); c.style.opacity = 1; c.style.left = x + 'px'; c.style.top = y + 'px'; await sleep(900); }
async function click() {
  const r = $('ripple'), c = $('cursor');
  r.style.left = (parseFloat(c.style.left) + 4) + 'px'; r.style.top = (parseFloat(c.style.top) + 3) + 'px';
  r.classList.remove('go'); void r.offsetWidth; r.classList.add('go'); await sleep(380);
}
async function type(target, text, ms = 55) { for (const ch of text) { target.textContent += ch; await sleep(ms); } }
function page(id) { ['pg-out', 'pg-ms', 'pg-in'].forEach(p => $(p).classList.toggle('show', p === id)); }
function door(id) { ['door-ui', 'door-v1'].forEach(d => $(d).classList.toggle('on', d === id)); }
async function stamps(list) { $('stamps').innerHTML = ''; for (const s of list) { const d = document.createElement('span'); d.className = 'stamp'; d.textContent = s; $('stamps').appendChild(d); void d.offsetWidth; d.classList.add('show'); await sleep(420); } }
function roles(list) { ['r-reader', 'r-contrib', 'r-writer'].forEach(r => $(r).classList.toggle('on', list.includes(r))); }

let idx = 0;
function chapter(name, steps) { $('chapter').textContent = name; idx = 0; $('prog').innerHTML = '<i></i>'.repeat(steps); }
async function say(html) {
  idx++;
  [...$('prog').children].forEach((d, i) => { d.classList.toggle('on', i < idx - 1); d.classList.toggle('now', i === idx - 1); });
  const ln = $('line'); ln.style.opacity = 0; await sleep(200); ln.innerHTML = html; ln.style.opacity = 1;
}

function reset() {
  $('browser').classList.remove('away'); $('term').classList.add('away'); $('who').textContent = 'your browser';
  page('pg-out'); $('url').textContent = 'localhost:7029/todos';
  $('ms-email').textContent = ''; $('ms-pass').textContent = ''; $('ms-pass').hidden = true; $('ms-email').hidden = false;
  $('ms-btn').textContent = 'Next'; $('ms-title').textContent = 'Sign in';
  $('todo-in').textContent = ''; $('new-item').classList.add('hide'); $('toast').classList.remove('show');
  $('stamps').innerHTML = ''; door(null); roles([]); $('entra').classList.remove('on'); $('store').classList.remove('on');
  $('cursor').style.opacity = 1; $('cursor').style.left = '480px'; $('cursor').style.top = '440px';
}
async function signIn() {
  chapter('Sign in', 6); reset();
  await say('Alex opens the Todos page. Not signed in yet.'); await sleep(1500);
  await cursorTo(404, 115); await click();
  await say('Alex clicks <em>Sign in</em>. The server sends the browser to Microsoft.');
  door('door-ui'); await send('rt-ui', 'plain', 'GET /authentication/login'); await send('rt-ui', 'plain', '302 → Entra ID', true); door(null);
  await say('Alex signs in on <em>Microsoft\'s page</em>. The app never sees the password.');
  $('entra').classList.add('on'); await send('rt-en', 'plain', 'authorize');
  $('url').textContent = 'login.microsoftonline.com'; page('pg-ms'); await sleep(500);
  await cursorTo(230, 266); await click(); await type($('ms-email'), 'alex@contoso.com');
  await cursorTo(330, 312); await click(); $('ms-email').hidden = true; $('ms-pass').hidden = false; $('ms-title').textContent = 'Enter password'; $('ms-btn').textContent = 'Sign in'; await sleep(300);
  await cursorTo(230, 266); await click(); await type($('ms-pass'), '••••••••••', 60);
  await cursorTo(330, 312); await click();
  await say('Entra ID answers with an <em>ID token</em> that lists Alex\'s roles.');
  roles(['r-reader', 'r-contrib']); await sleep(500);
  await send('rt-en', 'token', 'ID token', true); $('entra').classList.remove('on');
  await say('The server checks the token and hands back an <em>HttpOnly cookie</em>. The browser app never holds a token.');
  door('door-ui'); await send('rt-ui', 'token', 'POST /signin-oidc');
  await stamps(['signature ✓', 'audience ✓', 'roles ✓']);
  await send('rt-ui', 'cookie', 'cookie', true); door(null);
  $('url').textContent = 'localhost:7029/todos'; page('pg-in'); $('stamps').innerHTML = ''; await sleep(700);
  await say('Signed in as a <em>Contributor</em>, Alex can add todos.');
  await sleep(1400);
}
async function addTodo() {
  chapter('Add a todo', 3);
  await say('Alex types a todo and clicks <em>Add</em>.');
  await cursorTo(170, 176); await click(); await type($('todo-in'), 'Write the guide');
  await cursorTo(402, 176); await click();
  await say('The request goes through the <em>/api/ui</em> door with the cookie. The server checks the role.');
  door('door-ui'); await send('rt-ui', 'cookie', 'POST + cookie');
  await stamps(['cookie ✓', 'antiforgery ✓', 'Contributor ✓']); $('store').classList.add('on'); await sleep(500);
  await send('rt-ui', 'ok', '201 Created', true); door(null); $('store').classList.remove('on');
  $('todo-in').textContent = ''; $('new-item').classList.remove('hide'); $('new-item').classList.add('new'); $('toast').classList.add('show');
  await say('Saved. The new todo shows up in the list.');
  await sleep(1600); $('new-item').classList.remove('new'); $('toast').classList.remove('show'); await sleep(1600);
}
async function line(cls, text, ms = 28) { const d = document.createElement('div'); d.className = cls; $('tbody').appendChild(d); for (const ch of text) { d.textContent += ch; await sleep(ms); } }
async function consoleApp() {
  chapter('Another program', 4);
  $('cursor').style.opacity = 0; $('stamps').innerHTML = ''; roles([]);
  $('browser').classList.add('away'); $('term').classList.remove('away'); $('who').textContent = 'a console app'; $('tbody').innerHTML = ''; await sleep(600);
  await say('Now a <em>console app</em> wants the todos. Nobody clicks anything.');
  await line('p', '$ dotnet run'); await sleep(300);
  await say('It asks Entra ID for an <em>access token</em> for the API.');
  await line('d', 'Getting a token with DefaultAzureCredential…', 14);
  $('entra').classList.add('on'); await send('rt-en', 'plain', 'get token'); roles(['r-reader', 'r-writer']); await sleep(400);
  await send('rt-en', 'token', 'access token', true); $('entra').classList.remove('on');
  await line('d', '  scp access_as_user  roles Reader, Api.Writer', 10);
  await say('It knocks on the <em>/api/v1</em> door with the token. A cookie does not open this door.');
  await line('w', 'GET api/v1/todos', 22);
  door('door-v1'); await send('rt-v1', 'token', 'GET + Bearer token');
  await stamps(['audience ✓', 'scope ✓', 'Reader ✓']); $('store').classList.add('on'); await sleep(400);
  await send('rt-v1', 'ok', '200 OK', true); door(null); $('store').classList.remove('on');
  await say('The todos come back. Same data, a different door and a different key.');
  await line('p', '  -> 200 OK', 16); await line('', '  [x] Learn how prerendering works', 8); await line('', '  [ ] Build the todo API', 8); await line('', '  [ ] Write the guide', 8);
  await sleep(3600);
}
async function loop() {
  const my = ++gen;
  try { while (my === gen) { await signIn(); await addTodo(); await consoleApp(); } }
  catch (e) { if (!(e instanceof Stop)) throw e; }
}

reset(); chapter('Sign in', 6); $('line').textContent = 'Alex opens the Todos page. Not signed in yet.';
if (reduce) {
  // Reduced motion: one still frame that tells the main idea.
  page('pg-in'); $('new-item').classList.remove('hide'); roles(['r-reader', 'r-contrib']); $('cursor').style.opacity = 0;
  $('chapter').textContent = 'In short'; $('prog').innerHTML = '';
  $('line').innerHTML = 'The web app uses the <em>/api/ui</em> door with a cookie. Other programs use the <em>/api/v1</em> door with an Entra ID access token.';
} else {
  io = new IntersectionObserver(es => { visible = es.some(e => e.isIntersecting); }, { threshold: 0.25 }); io.observe(vp);
  loop();
}

stopAll = () => { gen++; ro?.disconnect(); io?.disconnect() }
})
onBeforeUnmount(() => stopAll())
</script>

<template>
  <div ref="root" class="scene">
    <div class="viewport" id="sc-vp">
      <div class="stage" id="sc-stage">
        <div class="label" style="left:24px;top:44px" id="sc-who">your browser</div>
        <div class="label" style="left:560px;top:142px">BlazorWebApplication.Server</div>
        <div class="label" style="left:812px;top:30px">Microsoft</div>

        <div class="win browser" id="sc-browser">
          <div class="bbar"><span class="d"></span><span class="d"></span><span class="d"></span><div class="url"><span class="lock"></span><span id="sc-url">localhost:7029/todos</span></div></div>
          <div class="page show" id="sc-pg-out">
            <div class="appbar">BlazorWebApplication<span class="sp"></span><span class="badge">WebAssembly</span><span class="btn">Sign in</span></div>
            <div class="appbody"><h4>Todos</h4><p class="muted">Sign in to see and change your todos.</p></div>
          </div>
          <div class="page mspage" id="sc-pg-ms">
            <div class="mscard">
              <div class="ms"><i></i><i></i><i></i><i></i></div>
              <h5 id="sc-ms-title">Sign in</h5>
              <div class="msinput" id="sc-ms-email" data-ph="Email, phone, or Skype"></div>
              <div class="msinput" id="sc-ms-pass" data-ph="Password" hidden></div>
              <span class="msbtn" id="sc-ms-btn">Next</span>
            </div>
          </div>
          <div class="page" id="sc-pg-in">
            <div class="appbar">BlazorWebApplication<span class="sp"></span><span class="who">alex@contoso.com</span><span class="btn">Sign out</span></div>
            <div class="appbody">
              <h4>Todos</h4>
              <div class="row"><div class="input" id="sc-todo-in" data-ph="What needs doing?"></div><span class="addbtn">+ Add</span></div>
              <div class="item done"><i class="box"></i><span>Learn how prerendering works</span></div>
              <div class="item"><i class="box"></i><span>Build the todo API</span></div>
              <div class="item hide" id="sc-new-item"><i class="box"></i><span>Write the guide</span></div>
            </div>
            <div class="toast" id="sc-toast">Todo added</div>
          </div>
        </div>

        <div class="win term away" id="sc-term">
          <div class="bbar"><span class="d"></span><span class="d"></span><span class="d"></span><span class="ttl">console-client</span></div>
          <div class="tbody" id="sc-tbody"></div>
        </div>

        <div class="server" id="sc-server">
          <div class="roof">Server</div>
          <div class="sub">ASP.NET Core</div>
          <div class="door ui" id="sc-door-ui">/api/ui<small>cookie only</small></div>
          <div class="door v1" id="sc-door-v1">/api/v1<small>token only</small></div>
          <div class="store" id="sc-store">todo<br>store</div>
        </div>
        <div class="stamps" id="sc-stamps"></div>

        <div class="entra" id="sc-entra">
          <div class="ms"><i></i><i></i><i></i><i></i></div>
          <div class="t">Entra ID</div>
          <div class="s">signs people in,<br>issues tokens</div>
          <div class="roles"><span class="role" id="sc-r-reader">Reader</span><span class="role" id="sc-r-contrib">Contributor</span><span class="role" id="sc-r-writer">Api.Writer</span></div>
        </div>

        <svg class="fx" viewBox="0 0 1000 466" aria-hidden="true">
          <path id="sc-rt-ui" class="route" d="M454 270 C 500 270, 510 268, 560 268"/>
          <path id="sc-rt-v1" class="route" d="M454 350 C 500 350, 510 346, 560 346"/>
          <path id="sc-rt-en" class="route" d="M330 70 C 330 6, 760 0, 812 110"/>
          <g id="sc-trails"></g>
          <g id="sc-pk" class="pk" opacity="0"></g>
        </svg>

        <div class="ripple" id="sc-ripple"></div>
        <div class="cursor" id="sc-cursor"><svg viewBox="0 0 24 24"><path d="M4 2l15 11-7 1 4 8-3 1-4-8-5 5z" fill="#fff" stroke="#08203f" stroke-width="1.5" stroke-linejoin="round"/></svg></div>
      </div>
    </div>

    <div class="subtitle" aria-live="polite">
      <span class="chapter" id="sc-chapter">Sign in</span>
      <p class="line" id="sc-line"></p>
    </div>
    <div class="prog" id="sc-prog" aria-hidden="true"></div>
  </div>
</template>

<style>
.scene {
  --bp: #0f3b6e; --ink: #eaf2ff; --ink-soft: #a9c6ea; --line: rgba(143,194,255,.16); --line-major: rgba(143,194,255,.3);
  --violet: #9b87ff; --blue: #5fb0ff; --green: #5fe0a0; --amber: #ffc45c;
  --display: 'Archivo', 'IBM Plex Sans', system-ui, sans-serif;
  --body: 'IBM Plex Sans', system-ui, -apple-system, 'Segoe UI', sans-serif;
  --mono: 'IBM Plex Mono', ui-monospace, Menlo, monospace;
  color: var(--ink);
}
.scene *, .scene *::before, .scene *::after { box-sizing: border-box; }
.scene p { margin: 0; }

.scene .viewport {position: relative; width: 100%;}
.scene .stage {position: absolute; left: 0; top: 0; width: 1000px; height: 466px; transform-origin: 0 0;}
.scene .fx {position: absolute; inset: 0; overflow: visible;}
.scene .route {fill: none; stroke: var(--line-major); stroke-width: 2; stroke-dasharray: 2 7; stroke-linecap: round;}
.scene .trail {fill: none; stroke-width: 3; stroke-linecap: round; transition: opacity .8s;}
.scene .trail.violet {stroke: var(--violet);}
.scene .trail.blue {stroke: var(--blue);}
.scene .trail.green {stroke: var(--green);}
.scene .trail.white {stroke: var(--ink);}
.scene .glow {filter: drop-shadow(0 0 6px currentColor);}
.scene .pk text {font: 600 12px var(--mono);}
.scene .label {position: absolute; font: 500 12px var(--mono); color: var(--ink-soft);}
.scene .server {position: absolute; left: 560px; top: 168px; width: 200px; height: 236px; border: 2px solid var(--ink-soft); border-radius: 14px; background: rgba(10, 44, 85, .7);}
.scene .server .roof {position: absolute; left: 16px; top: 14px; font: 700 17px var(--display); font-stretch: 112%;}
.scene .server .sub {position: absolute; left: 16px; top: 38px; font: 12px var(--mono); color: var(--ink-soft);}
.scene .door {position: absolute; left: -2px; width: 92px; height: 58px; border: 2px solid; border-left: none; border-radius: 0 10px 10px 0; padding: 8px 10px; font: 600 12.5px/1.3 var(--mono); transition: background-color .3s, box-shadow .3s;}
.scene .door small {display: block; font: 400 11px var(--body); color: var(--ink-soft); margin-top: 3px;}
.scene .door.ui {top: 74px; border-color: var(--violet); color: var(--violet);}
.scene .door.v1 {top: 152px; border-color: var(--blue); color: var(--blue);}
.scene .door.on.ui {background: rgba(155, 135, 255, .18); box-shadow: 0 0 22px rgba(155, 135, 255, .45);}
.scene .door.on.v1 {background: rgba(95, 176, 255, .18); box-shadow: 0 0 22px rgba(95, 176, 255, .45);}
.scene .store {position: absolute; right: 14px; top: 108px; width: 78px; height: 64px; border: 1.5px dashed var(--ink-soft); border-radius: 10px; font: 11px/1.3 var(--mono); color: var(--ink-soft); display: grid; place-items: center; text-align: center; transition: border-color .3s, color .3s;}
.scene .store.on {border-color: var(--green); color: var(--green);}
.scene .stamps {position: absolute; left: 560px; top: 414px; width: 380px; display: flex; flex-wrap: wrap; gap: 6px;}
.scene .stamp {font: 600 11.5px var(--mono); padding: 3px 9px; border-radius: 6px; border: 1.5px solid var(--green); color: var(--green); background: rgba(95, 224, 160, .1); opacity: 0; transform: scale(1.4) rotate(-4deg); transition: opacity .2s, transform .25s cubic-bezier(.2, 1.6, .4, 1);}
.scene .stamp.show {opacity: 1; transform: none;}
.scene .entra {position: absolute; left: 812px; top: 56px; width: 170px; height: 196px; border: 2px solid var(--blue); border-radius: 14px; background: rgba(10, 44, 85, .7); padding: 14px; transition: box-shadow .3s;}
.scene .entra.on {box-shadow: 0 0 30px rgba(95, 176, 255, .45);}
.scene .entra .t {font: 700 17px/1.15 var(--display); font-stretch: 112%; margin-top: 10px;}
.scene .entra .s {font: 12px var(--mono); color: var(--ink-soft); margin-top: 4px;}
.scene .ms {display: grid; grid-template-columns: 12px 12px; gap: 2px;}
.scene .ms i {width: 12px; height: 12px; display: block;}
.scene .ms i:nth-child(1) {background: #f25022;}
.scene .ms i:nth-child(2) {background: #7fba00;}
.scene .ms i:nth-child(3) {background: #00a4ef;}
.scene .ms i:nth-child(4) {background: #ffb900;}
.scene .roles {display: flex; flex-wrap: wrap; gap: 4px; margin-top: 12px;}
.scene .role {font: 600 10.5px var(--mono); padding: 2px 7px; border-radius: 999px; border: 1px solid var(--blue); color: var(--blue); opacity: .35; transition: opacity .3s, background-color .3s, color .3s;}
.scene .role.on {opacity: 1; background: var(--blue); color: #08203f;}
.scene .win {position: absolute; left: 24px; top: 70px; width: 430px; height: 330px; border-radius: 12px; overflow: hidden; box-shadow: 0 30px 60px -20px rgba(0, 10, 30, .7); transition: opacity .5s, transform .5s;}
.scene .win.away {opacity: 0; transform: translateY(14px) scale(.98);}
.scene .browser {background: #fff; color: #102a43;}
.scene .bbar {height: 34px; background: #e6edf5; display: flex; align-items: center; gap: 6px; padding: 0 10px;}
.scene .bbar .d {width: 9px; height: 9px; border-radius: 50%; background: #c3cedb;}
.scene .url {margin-left: 8px; flex: 1; height: 22px; border-radius: 6px; background: #fff; font: 12px/22px var(--mono); color: #3e5670; padding: 0 8px; white-space: nowrap; overflow: hidden; display: flex; gap: 6px; align-items: center;}
.scene .url .lock {width: 9px; height: 9px; border: 1.5px solid #18875a; border-radius: 2px; flex: none;}
.scene .page {position: absolute; left: 0; right: 0; top: 34px; bottom: 0; opacity: 0; transition: opacity .35s;}
.scene .page.show {opacity: 1;}
.scene .appbar {height: 40px; background: #594ae2; color: #fff; display: flex; align-items: center; gap: 8px; padding: 0 12px; font: 600 13px var(--body);}
.scene .appbar .sp {flex: 1;}
.scene .appbar .btn {border: 1px solid rgba(255,255,255,.65); border-radius: 6px; padding: 2px 8px; font-size: 12px; white-space: nowrap;}
.scene .appbar .who {font-weight: 500; font-size: 12px; white-space: nowrap;}
.scene .badge {font: 600 10px var(--body); background: #00c853; color: #fff; padding: 1px 7px; border-radius: 999px;}
.scene .appbody {padding: 14px 16px;}
.scene .appbody h4 {margin: 0 0 8px; font: 400 24px var(--body);}
.scene .muted {color: #5c7088; font-size: 13px; margin: 0;}
.scene .row {display: flex; gap: 8px; margin-bottom: 8px;}
.scene .input {flex: 1; height: 32px; border: 1px solid #c9d5e3; border-radius: 6px; padding: 0 10px; font: 13px/30px var(--body); background: #fff; white-space: nowrap; overflow: hidden;}
.scene .input:empty::before {content: attr(data-ph); color: #8a9bb0;}
.scene .addbtn {height: 32px; padding: 0 12px; border-radius: 6px; background: #594ae2; color: #fff; font: 600 12px/32px var(--body);}
.scene .item {display: flex; align-items: center; gap: 10px; padding: 7px 4px; font-size: 13px; border-radius: 6px; transition: background-color .8s;}
.scene .item .box {width: 14px; height: 14px; border: 2px solid #594ae2; border-radius: 3px;}
.scene .item.done .box {background: #594ae2;}
.scene .item.done span {text-decoration: line-through; color: #8a9bb0;}
.scene .item.new {background: #fff1cc;}
.scene .item.hide {display: none;}
.scene .toast {position: absolute; right: 12px; top: 48px; background: #18875a; color: #fff; font: 600 12px var(--body); padding: 6px 12px; border-radius: 6px; opacity: 0; transform: translateY(-6px); transition: opacity .3s, transform .3s;}
.scene .toast.show {opacity: 1; transform: none;}
.scene .mspage {background: #f2f2f2; display: grid; place-items: center;}
.scene .mscard {width: 300px; background: #fff; padding: 22px 26px; box-shadow: 0 2px 6px rgba(0,0,0,.15);}
.scene .mscard h5 {margin: 12px 0 10px; font: 600 20px var(--body); color: #1b1b1b;}
.scene .msinput {height: 30px; border-bottom: 1px solid #666; font: 14px/30px var(--body); color: #1b1b1b; margin-bottom: 14px; white-space: nowrap; overflow: hidden;}
.scene .msinput:empty::before {content: attr(data-ph); color: #767676;}
.scene .msbtn {float: right; background: #0067b8; color: #fff; font: 600 13px/30px var(--body); padding: 0 22px;}
.scene .mscard::after {content: ""; display: block; clear: both;}
.scene .term {background: #071a33; color: #cfe0f5; font: 12.5px/1.7 var(--mono);}
.scene .term .bbar {background: #0f2a50;}
.scene .term .bbar .d {background: #2c4d7a;}
.scene .term .ttl {color: #8fb3db; font-size: 12px; margin-left: 8px;}
.scene .tbody {padding: 12px 14px; white-space: pre-wrap;}
.scene .tbody .p {color: #7fd1a6;}
.scene .tbody .d {color: #8fb3db;}
.scene .tbody .w {color: #fff;}
.scene .cursor {position: absolute; left: 480px; top: 470px; width: 24px; height: 24px; z-index: 9; transition: left .85s cubic-bezier(.4,0,.2,1), top .85s cubic-bezier(.4,0,.2,1), opacity .3s; pointer-events: none;}
.scene .cursor svg {width: 24px; height: 24px; filter: drop-shadow(0 3px 3px rgba(0,0,0,.45));}
.scene .ripple {position: absolute; width: 34px; height: 34px; margin: -17px 0 0 -17px; border-radius: 50%; border: 2px solid #ffc45c; opacity: 0; z-index: 9; pointer-events: none;}
.scene .ripple.go {animation: sc-rip .5s ease-out;}
@keyframes sc-rip { from { opacity: 1; transform: scale(.3); } to { opacity: 0; transform: scale(1.5); } }
.scene .subtitle {display: flex; align-items: center; gap: 14px; margin-top: 18px; min-height: 52px;}
.scene .chapter {flex: none; font: 600 12px var(--mono); color: #08203f; background: var(--amber); border-radius: 6px; padding: 4px 10px;}
.scene .line {margin: 0; font: 500 clamp(15px, 1.7vw, 19px)/1.4 var(--body); color: var(--ink); text-wrap: balance; transition: opacity .3s;}
.scene .line em {font-style: normal; color: var(--amber);}
.scene .prog {display: flex; gap: 4px; margin-top: 14px;}
.scene .prog i {flex: 1; height: 3px; border-radius: 2px; background: var(--line-major); transition: background-color .4s;}
.scene .prog i.on {background: var(--ink-soft);}
.scene .prog i.now {background: var(--amber);}
@media (max-width: 640px) {
  .scene .subtitle {flex-direction: column; align-items: flex-start; gap: 8px;}
}
@media (prefers-reduced-motion: reduce) {
  .scene .win, .scene .page, .scene .cursor, .scene .stamp, .scene .door, .scene .role, .scene .toast, .scene .trail, .scene .line {transition: none !important;}
  .scene .ripple.go {animation: none;}
}
</style>
