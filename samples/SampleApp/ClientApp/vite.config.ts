import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [svelte()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': 'http://localhost:5000',
    }
  },
  // Add explicit build options to reduce file generation issues
  build: {
    rollupOptions: {
      output: {
        manualChunks: undefined
      }
    }
  },
  // Disable source maps in development to reduce file generation
  esbuild: {
    sourcemap: false
  }
})
