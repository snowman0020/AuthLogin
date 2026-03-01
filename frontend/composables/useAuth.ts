import { useAuthStore } from '~/stores/auth'
import type { AuthResponse, LoginRequest, RegisterRequest } from '~/types/auth'

export const useAuth = () => {
  const authStore = useAuthStore()
  const config = useRuntimeConfig()
  const router = useRouter()

  const apiBase = config.public.apiBase

  // ─── Core fetch with auth header ─────────────────────────────────────────────

  const authFetch = async <T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> => {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string> ?? {})
    }

    if (authStore.accessToken) {
      headers['Authorization'] = `Bearer ${authStore.accessToken}`
    }

    const res = await fetch(`${apiBase}${endpoint}`, {
      ...options,
      headers
    })

    if (!res.ok) {
      const err = await res.json().catch(() => ({ message: res.statusText }))
      throw new Error(err.message ?? 'Request failed')
    }

    return res.json()
  }

  // ─── Auth actions ─────────────────────────────────────────────────────────────

  const register = async (data: RegisterRequest): Promise<AuthResponse> => {
    const result = await authFetch<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data)
    })
    authStore.setAuth(result)
    return result
  }

  const login = async (data: LoginRequest): Promise<AuthResponse> => {
    const result = await authFetch<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(data)
    })
    authStore.setAuth(result)
    return result
  }

  const logout = async () => {
    try {
      if (authStore.refreshToken) {
        await authFetch('/auth/logout', {
          method: 'POST',
          body: JSON.stringify({ refreshToken: authStore.refreshToken })
        })
      }
    } finally {
      authStore.clearAuth()
      router.push('/login')
    }
  }

  const refreshAccessToken = async (): Promise<boolean> => {
    const token = authStore.refreshToken
    if (!token) return false

    try {
      const result = await fetch(`${apiBase}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: token })
      })

      if (!result.ok) {
        authStore.clearAuth()
        return false
      }

      const data: AuthResponse = await result.json()
      authStore.setAuth(data)
      return true
    } catch {
      authStore.clearAuth()
      return false
    }
  }

  return {
    register,
    login,
    logout,
    refreshAccessToken,
    authFetch,
    isAuthenticated: computed(() => authStore.isAuthenticated),
    user: computed(() => authStore.user),
    isAdmin: computed(() => authStore.isAdmin)
  }
}
