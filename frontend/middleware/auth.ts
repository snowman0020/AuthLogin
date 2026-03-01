import { useAuthStore } from '~/stores/auth'

export default defineNuxtRouteMiddleware(async (to) => {
  const authStore = useAuthStore()

  // Load tokens from localStorage on client side
  if (import.meta.client) {
    authStore.loadFromStorage()
  }

  const publicRoutes = ['/login', '/register']
  const isPublic = publicRoutes.includes(to.path)

  if (!authStore.isAuthenticated) {
    if (!isPublic) {
      return navigateTo('/login')
    }
    return
  }

  // Redirect authenticated users away from login/register
  if (isPublic) {
    return navigateTo('/dashboard')
  }
})
