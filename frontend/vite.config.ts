import { defineConfig } from 'vite';
import obfuscator from 'vite-plugin-javascript-obfuscator';

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
  plugins: [
    {
      ...obfuscator({
        options: {
          compact: true,
          controlFlowFlattening: true,
          deadCodeInjection: true,
          stringArray: true,
          rotateStringArray: true,
          selfDefending: false,
          disableConsoleOutput: false,
        },
      }),
      apply: 'build',
    },
  ],
});
