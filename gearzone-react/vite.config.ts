import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      // Proxy API requests to ASP.NET backend during development
      '/api': {
        target: 'https://localhost:7266',
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: 'https://localhost:7266',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});
