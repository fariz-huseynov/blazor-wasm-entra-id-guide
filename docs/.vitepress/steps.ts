// The 12 steps of the guide. Used by the sidebar, the landing page timeline and the step progress bar.
export interface Step {
  n: string
  slug: string
  title: string
  short: string
  summary: string
  part: 1 | 2
}

export const steps: Step[] = [
  { n: '01', slug: '01-create-project', part: 1, title: 'Create the project', short: 'Template',
    summary: 'Create a Blazor Web App that runs its UI in the browser with WebAssembly.' },
  { n: '02', slug: '02-add-shared-project', part: 1, title: 'Add a shared project', short: 'Shared',
    summary: 'Add a class library with the todo model and a service interface for server and browser.' },
  { n: '03', slug: '03-add-mudblazor', part: 1, title: 'Add MudBlazor', short: 'MudBlazor',
    summary: 'Replace Bootstrap with MudBlazor and give the app an app bar and a side menu.' },
  { n: '04', slug: '04-show-render-mode', part: 1, title: 'Show the render mode', short: 'Render mode',
    summary: 'Add a badge that shows whether the page is prerendered or running in WebAssembly.' },
  { n: '05', slug: '05-todos', part: 1, title: 'Add a todo list', short: 'Todos',
    summary: 'Add a Todos page that works the same way on the server and in the browser.' },
  { n: '06', slug: '06-external-api', part: 1, title: 'Add an external API', short: 'API v1',
    summary: 'Add a versioned API at /api/v1/todos for other programs.' },
  { n: '07', slug: '07-openapi-scalar', part: 1, title: 'OpenAPI and Scalar', short: 'Scalar',
    summary: 'Describe /api/v1 with OpenAPI and try it in the Scalar API reference.' },
  { n: '08', slug: '08-entra-sign-in', part: 2, title: 'Sign in with Entra ID', short: 'Sign-in',
    summary: 'Register the app in Microsoft Entra ID and let users sign in and out.' },
  { n: '09', slug: '09-app-roles', part: 2, title: 'Use app roles in the UI', short: 'App roles',
    summary: 'Define Reader, Contributor and Api.Writer, and let roles decide what users can do.' },
  { n: '10', slug: '10-protect-ui-api', part: 2, title: 'Protect the UI API', short: 'UI API',
    summary: 'Accept only signed-in users with the right role, plus an antiforgery check on changes.' },
  { n: '11', slug: '11-protect-external-api', part: 2, title: 'Protect the external API', short: 'Bearer',
    summary: 'Accept only Entra ID access tokens on /api/v1 and try it in Scalar.' },
  { n: '12', slug: '12-console-client', part: 2, title: 'Call the API from a console app', short: 'Console',
    summary: 'Get a token with DefaultAzureCredential and call the API as you or as a service principal.' },
]

export const repo = 'https://github.com/fariz-huseynov/blazor-wasm-entra-id-guide'
