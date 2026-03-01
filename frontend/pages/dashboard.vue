<template>
  <div>
    <!-- Navbar -->
    <nav class="navbar">
      <span class="logo">Auth App</span>
      <div class="nav-actions">
        <span style="font-size: 0.875rem; color: #94a3b8;">
          {{ user?.email }}
        </span>
        <button class="btn btn-outline" style="color: #fff; border-color: #64748b;"
          @click="handleLogout">
          Sign Out
        </button>
      </div>
    </nav>

    <div class="container" style="padding-top: 2rem; padding-bottom: 3rem;">
      <!-- Welcome -->
      <div class="card" style="margin-bottom: 1.5rem;">
        <h2 style="font-size: 1.5rem; margin-bottom: 0.5rem;">
          Welcome back, {{ user?.username }}!
        </h2>
        <p style="color: #64748b; font-size: 0.9rem;">
          Role: <strong>{{ user?.role }}</strong> &nbsp;|&nbsp;
          ID: <code style="font-size: 0.8rem;">{{ user?.id }}</code>
        </p>
      </div>

      <!-- Token Info -->
      <div class="card" style="margin-bottom: 1.5rem;">
        <h3 style="margin-bottom: 1rem;">JWT Access Token</h3>
        <textarea
          readonly
          :value="authStore.accessToken ?? ''"
          style="width:100%; height:100px; font-size:0.75rem; font-family:monospace;
                 border:1px solid #e2e8f0; border-radius:8px; padding:0.75rem; resize:none;"
        />
        <button class="btn btn-outline" style="margin-top: 0.75rem;" @click="copyToken">
          {{ copied ? 'Copied!' : 'Copy Token' }}
        </button>
      </div>

      <!-- API Keys -->
      <div class="card">
        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:1rem;">
          <h3>API Keys</h3>
          <button class="btn btn-primary" @click="showCreateKey = true">
            + New API Key
          </button>
        </div>

        <!-- Create Key Modal -->
        <div v-if="showCreateKey" class="modal-overlay" @click.self="showCreateKey = false">
          <div class="card" style="width:100%; max-width:420px;">
            <h4 style="margin-bottom:1rem;">Create API Key</h4>
            <div class="form-group">
              <label>Name</label>
              <input v-model="newKeyName" placeholder="My API Key" />
            </div>
            <div style="display:flex; gap:0.75rem;">
              <button class="btn btn-primary" :disabled="!newKeyName || creatingKey"
                @click="createKey">
                {{ creatingKey ? 'Creating...' : 'Create' }}
              </button>
              <button class="btn btn-outline" @click="showCreateKey = false">Cancel</button>
            </div>
          </div>
        </div>

        <!-- New Key Display -->
        <div v-if="newKeyValue" class="alert alert-success" style="margin-bottom:1rem;">
          <strong>New API Key (copy now, shown only once):</strong><br />
          <code style="word-break:break-all;">{{ newKeyValue }}</code>
        </div>

        <!-- Keys Table -->
        <div v-if="apiKeys.length === 0" style="color:#64748b; font-size:0.9rem;">
          No API keys yet. Create one above.
        </div>

        <table v-else class="keys-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Key (prefix)</th>
              <th>Created</th>
              <th>Last Used</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="key in apiKeys" :key="key.id">
              <td>{{ key.name }}</td>
              <td><code>{{ key.key.slice(0, 16) }}...</code></td>
              <td>{{ formatDate(key.createdAt) }}</td>
              <td>{{ key.lastUsedAt ? formatDate(key.lastUsedAt) : '—' }}</td>
              <td>
                <span :class="key.isActive ? 'badge-active' : 'badge-inactive'">
                  {{ key.isActive ? 'Active' : 'Revoked' }}
                </span>
              </td>
              <td>
                <button v-if="key.isActive" class="btn btn-danger"
                  style="padding: 0.3rem 0.75rem; font-size:0.8rem;"
                  @click="revokeKey(key.id)">
                  Revoke
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import type { ApiKeyResponse } from '~/types/auth'

definePageMeta({ middleware: ['auth'] })

const { logout, authFetch, user } = useAuth()
const authStore = useAuthStore()
const config = useRuntimeConfig()

const apiKeys = ref<ApiKeyResponse[]>([])
const showCreateKey = ref(false)
const newKeyName = ref('')
const newKeyValue = ref('')
const creatingKey = ref(false)
const copied = ref(false)

onMounted(async () => {
  await loadApiKeys()
})

const loadApiKeys = async () => {
  try {
    const keys = await authFetch<ApiKeyResponse[]>('/apikey')
    apiKeys.value = keys
  } catch {}
}

const createKey = async () => {
  creatingKey.value = true
  try {
    const key = await authFetch<ApiKeyResponse>('/apikey', {
      method: 'POST',
      body: JSON.stringify({ name: newKeyName.value, expiresAt: null })
    })
    newKeyValue.value = key.key
    showCreateKey.value = false
    newKeyName.value = ''
    await loadApiKeys()
  } catch (e: unknown) {
    alert(e instanceof Error ? e.message : 'Failed to create key')
  } finally {
    creatingKey.value = false
  }
}

const revokeKey = async (id: string) => {
  if (!confirm('Revoke this API key?')) return
  try {
    await authFetch(`/apikey/${id}`, { method: 'DELETE' })
    await loadApiKeys()
  } catch {}
}

const handleLogout = () => logout()

const copyToken = async () => {
  if (!authStore.accessToken) return
  await navigator.clipboard.writeText(authStore.accessToken)
  copied.value = true
  setTimeout(() => (copied.value = false), 2000)
}

const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('th-TH', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  padding: 1rem;
}

.keys-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.keys-table th, .keys-table td {
  text-align: left;
  padding: 0.65rem 0.75rem;
  border-bottom: 1px solid #f1f5f9;
}

.keys-table th {
  color: #64748b;
  font-weight: 600;
  background: #f8fafc;
}

.badge-active {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 99px;
  background: #dcfce7;
  color: #15803d;
  font-size: 0.75rem;
  font-weight: 600;
}

.badge-inactive {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 99px;
  background: #fee2e2;
  color: #b91c1c;
  font-size: 0.75rem;
  font-weight: 600;
}
</style>
