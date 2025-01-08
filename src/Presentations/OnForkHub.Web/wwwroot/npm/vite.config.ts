import { resolve } from 'path'
import { defineConfig } from 'vite'

export default defineConfig({
    build: {
        outDir: '../js',
        emptyOutDir: true,
        lib: {
            entry: resolve(__dirname, './src/main.ts'),
            formats: ['es'],
            name: 'mylib',
            fileName: () => 'main.min.js'
        },
        rollupOptions: {
            external: ['webtorrent', 'events', 'buffer', 'stream', 'util', 'os', 'crypto', 'path', 'http', 'https', 'zlib', 'net', 'tls', 'fs', 'dns', 'dgram'],
            output: {
                format: 'es',
                compact: true,
                globals: {
                    webtorrent: 'WebTorrent'
                }
            }
        }
    },
    resolve: {
        alias: {
            '@': resolve(__dirname, './src')
        }
    }
})