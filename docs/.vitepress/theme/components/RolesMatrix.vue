<script setup lang="ts">
import { ref } from 'vue'

type Role = 'Reader' | 'Contributor' | 'Api.Writer'
const roles: { name: Role, who: string, about: string }[] = [
  { name: 'Reader', who: 'Users and apps', about: 'Can look, can\'t change anything.' },
  { name: 'Contributor', who: 'Users only', about: 'Works with todos in the web app.' },
  { name: 'Api.Writer', who: 'Users and apps', about: 'Changes todos through the external API.' },
]
const rows: { action: string, where: string, allowed: Role[] }[] = [
  { action: 'See the todo list', where: 'Web app, /api/ui', allowed: ['Reader', 'Contributor'] },
  { action: 'Add, tick or delete todos', where: 'Web app, /api/ui', allowed: ['Contributor'] },
  { action: 'Read todos', where: 'External API, /api/v1', allowed: ['Reader', 'Api.Writer'] },
  { action: 'Create, update or delete todos', where: 'External API, /api/v1', allowed: ['Api.Writer'] },
]
const focus = ref<Role | null>(null)
const toggle = (r: Role) => (focus.value = focus.value === r ? null : r)
</script>

<template>
  <div class="roles">
    <div class="roles-pick" role="group" aria-label="Highlight a role">
      <button v-for="r in roles" :key="r.name" type="button" class="role-chip"
              :aria-pressed="focus === r.name" @click="toggle(r.name)">
        <span class="role-name">{{ r.name }}</span>
        <span class="role-who">{{ r.who }}</span>
      </button>
    </div>
    <div class="roles-table-wrap">
      <table class="roles-table" :class="{ focused: focus }">
        <thead>
          <tr>
            <th scope="col">What you want to do</th>
            <th v-for="r in roles" :key="r.name" scope="col" :class="{ hl: focus === r.name }">{{ r.name }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in rows" :key="row.action">
            <th scope="row"><span class="ra">{{ row.action }}</span><span class="rw">{{ row.where }}</span></th>
            <td v-for="r in roles" :key="r.name" :class="{ hl: focus === r.name, yes: row.allowed.includes(r.name) }">
              <span v-if="row.allowed.includes(r.name)" class="mark yes" aria-hidden="true">✓</span>
              <span v-else class="mark no" aria-hidden="true">✕</span>
              <span class="visually-hidden">{{ row.allowed.includes(r.name) ? 'allowed' : 'not allowed' }}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <p class="roles-note">
      No role at all? The web app shows an access-denied page, and both APIs answer <code>403</code>.
      Not signed in, or no token? You get <code>401</code>.
    </p>
  </div>
</template>
