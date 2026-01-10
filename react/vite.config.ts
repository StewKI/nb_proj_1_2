import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/gamehub': {
        target: 'http://npp-backend:8080',
        ws: true,
        changeOrigin: true
      },
      '/auth': {
        target: 'http://npp-backend:8080',
        changeOrigin: true
      }
    }
  }
})
