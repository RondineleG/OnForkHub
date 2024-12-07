import {resolve} from 'path'
import {BuildOptions, defineConfig, ResolveOptions} from 'vite'

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
            external: ['webtorrent'],
            output: {
                globals: {
                    'webtorrent': 'WebTorrent'
                },
                format: 'es',
                compact: true
            }
        }
    } as BuildOptions,
    resolve: {
        alias: {
            '@': resolve(__dirname, './src')
        }
    } as ResolveOptions,
    optimizeDeps: {
        include: ['webtorrent']
    }
});