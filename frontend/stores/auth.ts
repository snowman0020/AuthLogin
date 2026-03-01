import { defineStore } from 'pinia'
import type { UserResponse, AuthResponse } from '~/types/auth'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as UserResponse | null,
    accessToken: null as string | null,
    refreshToken: null as string | null,
    tokenExpiresAt: null as string | null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.accessToken && !!state.user,
    isAdmin: (state) => state.user?.role === 'Admin',
  },

  actions: {
    setAuth(data: AuthResponse) {
      this.accessToken = data.accessToken
      this.refreshToken = data.refreshToken
      this.tokenExpiresAt = data.expiresAt
      this.user = data.user

      if (import.meta.client) {
        localStorage.setItem('access_token', data.accessToken)
        localStorage.setItem('refresh_token', data.refreshToken)
        localStorage.setItem('token_expires_at', data.expiresAt)
        localStorage.setItem('user', JSON.stringify(data.user))
      }
    },

    loadFromStorage() {
      if (!import.meta.client) return
      const token = localStorage.getItem('access_token')
      const user = localStorage.getItem('user')
      if (token && user) {
        this.accessToken = token
        this.refreshToken = localStorage.getItem('refresh_token')
        this.tokenExpiresAt = localStorage.getItem('token_expires_at')
        this.user = JSON.parse(user)
      }
    },

    clearAuth() {
      this.accessToken = null
      this.refreshToken = null
      this.tokenExpiresAt = null
      this.user = null

      if (import.meta.client) {
        localStorage.removeItem('access_token')
        localStorage.removeItem('refresh_token')
        localStorage.removeItem('token_expires_at')
        localStorage.removeItem('user')
      }
    },

    isTokenExpired(): boolean {
      if (!this.tokenExpiresAt) return true
      return new Date(this.tokenExpiresAt) <= new Date()
    }
  }
})
