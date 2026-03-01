<template>
  <div style="display:flex; align-items:center; justify-content:center; min-height:100vh;">
    <p>Redirecting...</p>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: ['auth'] })
</script>
