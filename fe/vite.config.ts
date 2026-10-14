/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

// The browser calls /api/*; the dev server forwards it to the MyWeather API, so no CORS setup is needed.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // The API's https profile (the default in CLAUDE.md and the F5 config) serves https://localhost:7057 and
  // redirects plain http to it. Proxying straight to https avoids that redirect: the browser would follow it
  // to another origin and fail CORS. For the http-only profile set VITE_API_PROXY_TARGET=http://localhost:5020.
  const apiTarget = env.VITE_API_PROXY_TARGET || 'https://localhost:7057'

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
          // Accept the self-signed ASP.NET Core development certificate.
          secure: false,
          rewrite: (path) => path.replace(/^\/api/, ''),
        },
      },
    },
    test: {
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      css: false,
    },
  }
})
