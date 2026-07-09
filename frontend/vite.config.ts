import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5046', // Forward API requests to .NET backend API
        changeOrigin: true,
        secure: false,
      }
    }
  }
});
