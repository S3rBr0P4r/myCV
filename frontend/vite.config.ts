/// <reference types="vitest" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  root: 'src',
  envDir: '..',
  plugins: [react()],
  build: {
    outDir: '../dist',
    sourcemap: false,
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:60355',
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'happy-dom',
    globals: true,
    include: ['../tests/**/*.{test,spec}.?(c|m)[jt]s?(x)'],
    setupFiles: ['../tests/test-setup.ts'],
    css: true,
    env: {
      VITE_API_URL: 'http://localhost:60355/api/v1/cv',
    },
  },
});
