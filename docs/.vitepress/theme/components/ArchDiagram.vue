<template>
  <figure class="arch">
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

      <!-- Clients -->
      <g class="a-box a-client">
        <rect x="20" y="40" width="230" height="130" rx="10" />
        <text x="40" y="76" class="a-h">Browser</text>
        <text x="40" y="102" class="a-t">Blazor WebAssembly app</text>
        <text x="40" y="124" class="a-t">MudBlazor UI, roles from</text>
        <text x="40" y="144" class="a-t">the signed-in user</text>
      </g>
      <g class="a-box a-client">
        <rect x="20" y="290" width="230" height="130" rx="10" />
        <text x="40" y="326" class="a-h">Other systems</text>
        <text x="40" y="352" class="a-t">Console app, scripts,</text>
        <text x="40" y="372" class="a-t">services. Tokens from</text>
        <text x="40" y="392" class="a-t">DefaultAzureCredential</text>
      </g>

      <!-- Server -->
      <g class="a-box a-server">
        <rect x="350" y="20" width="300" height="420" rx="12" />
        <text x="370" y="54" class="a-h">ASP.NET Core server</text>
        <text x="370" y="76" class="a-t">BlazorWebApplication.Server</text>
      </g>
      <g class="a-port part-1">
        <rect x="370" y="100" width="260" height="104" rx="8" />
        <text x="388" y="132" class="a-code">/api/ui/todos</text>
        <text x="388" y="156" class="a-t">Sign-in cookie only</text>
        <text x="388" y="178" class="a-t">Antiforgery check on changes</text>
      </g>
      <g class="a-port part-2">
        <rect x="370" y="300" width="260" height="104" rx="8" />
        <text x="388" y="332" class="a-code">/api/v1/todos</text>
        <text x="388" y="356" class="a-t">Bearer token only</text>
        <text x="388" y="378" class="a-t">Documented in Scalar</text>
      </g>
      <g class="a-store">
        <rect x="430" y="224" width="140" height="54" rx="27" />
        <text x="500" y="256" class="a-t" text-anchor="middle">Todo store</text>
      </g>
      <path d="M500 204 V224 M500 278 V300" class="a-line thin" />

      <!-- Entra -->
      <g class="a-box a-entra">
        <rect x="750" y="120" width="190" height="220" rx="10" />
        <text x="770" y="156" class="a-h">Microsoft</text>
        <text x="770" y="180" class="a-h">Entra ID</text>
        <text x="770" y="212" class="a-t">One app registration</text>
        <text x="770" y="234" class="a-t">Roles: Reader,</text>
        <text x="770" y="256" class="a-t">Contributor, Api.Writer</text>
        <text x="770" y="278" class="a-t">Scope: access_as_user</text>
      </g>

      <!-- Flows -->
      <path d="M250 130 H370" class="a-line part-1" marker-end="url(#arr)" />
      <text x="262" y="120" class="a-label">cookie</text>

      <path d="M250 360 H370" class="a-line part-2" marker-end="url(#arr)" />
      <text x="262" y="350" class="a-label">token</text>

      <path d="M650 70 C 710 70, 720 140, 750 160" class="a-line dash" marker-start="url(#arr)" marker-end="url(#arr)" />
      <text x="664" y="62" class="a-label">sign-in, ID token</text>

      <path d="M135 420 V470 H805 Q845 470 845 346" class="a-line dash" marker-end="url(#arr)" />
      <text x="500" y="462" class="a-label" text-anchor="middle">get an access token (az login, managed identity, service principal)</text>
    </svg>
    <figcaption>
      Two APIs, one server: the web app uses <code>/api/ui</code> with its sign-in cookie,
      everything else uses <code>/api/v1</code> with an access token. Neither accepts the other's credentials.
    </figcaption>
  </figure>
</template>
