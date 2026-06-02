import { defineConfig } from 'vite';

export default defineConfig({
  root: 'src',
  envDir: '..',
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
});
