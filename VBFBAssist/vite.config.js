import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const safeProcessEnv = Object.keys(env)
    .filter((key) => key.startsWith('REACT_'))
    .reduce((acc, key) => {
      acc[key] = env[key];
      return acc;
    }, { NODE_ENV: mode });

  // Version is baked in at build time: CI sets REACT_APP_CURRENTVERSION=YY.MMDD.NN.<build>
  // (top entry of CHANGES.md + build number); local builds get YY.MMDD.HH.MM-local.
  const now = new Date();
  const pad = (n) => String(n).padStart(2, '0');
  safeProcessEnv.REACT_APP_CURRENTVERSION = process.env.REACT_APP_CURRENTVERSION ||
    `${String(now.getFullYear()).slice(2)}.${pad(now.getMonth() + 1)}${pad(now.getDate())}.${pad(now.getHours())}.${pad(now.getMinutes())}-local`;

  return {
    define: {
      'process.env': safeProcessEnv
    },
    plugins: [react()],
    server: {
      port: 5183,
      strictPort: true
    },
    build: {
      chunkSizeWarningLimit: 1600,
      // Release output alongside the other Tonic apps (ReadyWebApp builds to C:\TonicRelease\ReadySync)
      outDir: 'C:\\TonicRelease\\VBFBAssist',
      // outDir is outside the project, so Vite needs permission to clear old hashed bundles
      emptyOutDir: true
    }
  }
})
