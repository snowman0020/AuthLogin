<template>
  <div class="auth-page">
    <div class="card auth-card">
      <h1 class="auth-title">Create Account</h1>
      <p class="auth-sub">Sign up to get started today.</p>

      <div v-if="error" class="alert alert-error">{{ error }}</div>

      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label for="username">Username</label>
          <input id="username" v-model="form.username" type="text"
            placeholder="johndoe" required minlength="3" />
        </div>

        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" v-model="form.email" type="email"
            placeholder="you@example.com" required />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input id="password" v-model="form.password" type="password"
            placeholder="Min. 6 characters" required minlength="6" />
        </div>

        <button type="submit" class="btn btn-primary btn-full" :disabled="loading">
          {{ loading ? 'Creating account...' : 'Create Account' }}
        </button>
      </form>

      <p class="auth-footer">
        Already have an account?
        <NuxtLink to="/login">Sign in</NuxtLink>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  middleware: ['auth'],
  layout: 'blank'
})

const { register } = useAuth()
const router = useRouter()

const form = reactive({ username: '', email: '', password: '' })
const error = ref('')
const loading = ref(false)

const handleRegister = async () => {
  error.value = ''
  loading.value = true

  try {
    await register(form)
    router.push('/dashboard')
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Registration failed'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #1e293b 0%, #3b82f6 100%);
  padding: 2rem;
}
.auth-card { width: 100%; max-width: 420px; }
.auth-title { font-size: 1.75rem; font-weight: 700; color: #1e293b; margin-bottom: 0.25rem; }
.auth-sub { color: #64748b; margin-bottom: 1.75rem; font-size: 0.9rem; }
.auth-footer { text-align: center; margin-top: 1.25rem; font-size: 0.875rem; color: #64748b; }
.auth-footer a { color: #3b82f6; font-weight: 600; text-decoration: none; }
</style>
