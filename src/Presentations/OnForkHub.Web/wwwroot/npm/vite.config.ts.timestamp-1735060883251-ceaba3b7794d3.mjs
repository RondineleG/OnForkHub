// vite.config.ts
import { resolve } from "path";
import { defineConfig } from "file:///C:/Dev/OnForkHub/src/Presentations/OnForkHub.Web/wwwroot/npm/node_modules/vite/dist/node/index.js";
var __vite_injected_original_dirname = "C:\\Dev\\OnForkHub\\src\\Presentations\\OnForkHub.Web\\wwwroot\\npm";
var vite_config_default = defineConfig({
  build: {
    outDir: "../js",
    emptyOutDir: true,
    lib: {
      entry: resolve(__vite_injected_original_dirname, "./src/main.ts"),
      formats: ["es"],
      name: "mylib",
      fileName: () => "main.min.js"
    },
    rollupOptions: {
      external: ["webtorrent"],
      output: {
        globals: {
          "webtorrent": "WebTorrent"
        },
        format: "es",
        compact: true
      }
    }
  },
  resolve: {
    alias: {
      "@": resolve(__vite_injected_original_dirname, "./src")
    }
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJDOlxcXFxEZXZcXFxcT25Gb3JrSHViXFxcXHNyY1xcXFxQcmVzZW50YXRpb25zXFxcXE9uRm9ya0h1Yi5XZWJcXFxcd3d3cm9vdFxcXFxucG1cIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIkM6XFxcXERldlxcXFxPbkZvcmtIdWJcXFxcc3JjXFxcXFByZXNlbnRhdGlvbnNcXFxcT25Gb3JrSHViLldlYlxcXFx3d3dyb290XFxcXG5wbVxcXFx2aXRlLmNvbmZpZy50c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vQzovRGV2L09uRm9ya0h1Yi9zcmMvUHJlc2VudGF0aW9ucy9PbkZvcmtIdWIuV2ViL3d3d3Jvb3QvbnBtL3ZpdGUuY29uZmlnLnRzXCI7Ly8vIDxyZWZlcmVuY2UgdHlwZXM9XCJ2aXRlL2NsaWVudFwiIC8+XHJcbmltcG9ydCB7cmVzb2x2ZX0gZnJvbSAncGF0aCdcclxuaW1wb3J0IHtCdWlsZE9wdGlvbnMsIGRlZmluZUNvbmZpZywgUmVzb2x2ZU9wdGlvbnN9IGZyb20gJ3ZpdGUnXHJcblxyXG5leHBvcnQgZGVmYXVsdCBkZWZpbmVDb25maWcoe1xyXG4gICAgYnVpbGQ6IHtcclxuICAgICAgICBvdXREaXI6ICcuLi9qcycsXHJcbiAgICAgICAgZW1wdHlPdXREaXI6IHRydWUsXHJcbiAgICAgICAgbGliOiB7XHJcbiAgICAgICAgICAgIGVudHJ5OiByZXNvbHZlKF9fZGlybmFtZSwgJy4vc3JjL21haW4udHMnKSxcclxuICAgICAgICAgICAgZm9ybWF0czogWydlcyddLFxyXG4gICAgICAgICAgICBuYW1lOiAnbXlsaWInLFxyXG4gICAgICAgICAgICBmaWxlTmFtZTogKCkgPT4gJ21haW4ubWluLmpzJ1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcm9sbHVwT3B0aW9uczoge1xyXG4gICAgICAgICAgICBleHRlcm5hbDogWyd3ZWJ0b3JyZW50J10sXHJcbiAgICAgICAgICAgIG91dHB1dDoge1xyXG4gICAgICAgICAgICAgICAgZ2xvYmFsczoge1xyXG4gICAgICAgICAgICAgICAgICAgICd3ZWJ0b3JyZW50JzogJ1dlYlRvcnJlbnQnXHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICAgICAgZm9ybWF0OiAnZXMnLFxyXG4gICAgICAgICAgICAgICAgY29tcGFjdDogdHJ1ZVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfSBhcyBCdWlsZE9wdGlvbnMsXHJcbiAgICByZXNvbHZlOiB7XHJcbiAgICAgICAgYWxpYXM6IHtcclxuICAgICAgICAgICAgJ0AnOiByZXNvbHZlKF9fZGlybmFtZSwgJy4vc3JjJylcclxuICAgICAgICB9XHJcbiAgICB9IGFzIFJlc29sdmVPcHRpb25zXHJcbn0pIl0sCiAgIm1hcHBpbmdzIjogIjtBQUNBLFNBQVEsZUFBYztBQUN0QixTQUFzQixvQkFBbUM7QUFGekQsSUFBTSxtQ0FBbUM7QUFJekMsSUFBTyxzQkFBUSxhQUFhO0FBQUEsRUFDeEIsT0FBTztBQUFBLElBQ0gsUUFBUTtBQUFBLElBQ1IsYUFBYTtBQUFBLElBQ2IsS0FBSztBQUFBLE1BQ0QsT0FBTyxRQUFRLGtDQUFXLGVBQWU7QUFBQSxNQUN6QyxTQUFTLENBQUMsSUFBSTtBQUFBLE1BQ2QsTUFBTTtBQUFBLE1BQ04sVUFBVSxNQUFNO0FBQUEsSUFDcEI7QUFBQSxJQUNBLGVBQWU7QUFBQSxNQUNYLFVBQVUsQ0FBQyxZQUFZO0FBQUEsTUFDdkIsUUFBUTtBQUFBLFFBQ0osU0FBUztBQUFBLFVBQ0wsY0FBYztBQUFBLFFBQ2xCO0FBQUEsUUFDQSxRQUFRO0FBQUEsUUFDUixTQUFTO0FBQUEsTUFDYjtBQUFBLElBQ0o7QUFBQSxFQUNKO0FBQUEsRUFDQSxTQUFTO0FBQUEsSUFDTCxPQUFPO0FBQUEsTUFDSCxLQUFLLFFBQVEsa0NBQVcsT0FBTztBQUFBLElBQ25DO0FBQUEsRUFDSjtBQUNKLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==
