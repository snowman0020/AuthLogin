export default defineNuxtConfig({
  devtools: { enabled: true },

  modules: ['@pinia/nuxt'],

  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE ?? 'http://localhost:3000/api'
    }
  },

  typescript: { strict: true },

  css: ['~/assets/css/main.css'],

  devServer: { port: 3009 },

  app: {
    head: {
      title: 'Auth App',
      meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }]
    }
  }
})
