// vite.config.ts
import { resolve } from "path";
import { defineConfig } from "file:///C:/dev/OnForkHub/src/Presentations/OnForkHub.Web/wwwroot/npm/node_modules/vite/dist/node/index.js";
var __vite_injected_original_dirname = "C:\\dev\\OnForkHub\\src\\Presentations\\OnForkHub.Web\\wwwroot\\npm";
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
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJDOlxcXFxkZXZcXFxcT25Gb3JrSHViXFxcXHNyY1xcXFxQcmVzZW50YXRpb25zXFxcXE9uRm9ya0h1Yi5XZWJcXFxcd3d3cm9vdFxcXFxucG1cIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIkM6XFxcXGRldlxcXFxPbkZvcmtIdWJcXFxcc3JjXFxcXFByZXNlbnRhdGlvbnNcXFxcT25Gb3JrSHViLldlYlxcXFx3d3dyb290XFxcXG5wbVxcXFx2aXRlLmNvbmZpZy50c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vQzovZGV2L09uRm9ya0h1Yi9zcmMvUHJlc2VudGF0aW9ucy9PbkZvcmtIdWIuV2ViL3d3d3Jvb3QvbnBtL3ZpdGUuY29uZmlnLnRzXCI7Ly8vIDxyZWZlcmVuY2UgdHlwZXM9XCJ2aXRlL2NsaWVudFwiIC8+XHJcbmltcG9ydCB7IHJlc29sdmUgfSBmcm9tICdwYXRoJ1xyXG5pbXBvcnQgeyBkZWZpbmVDb25maWcsIEJ1aWxkT3B0aW9ucywgUmVzb2x2ZU9wdGlvbnMgfSBmcm9tICd2aXRlJ1xyXG5cclxuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKHtcclxuICAgIGJ1aWxkOiB7XHJcbiAgICAgICAgb3V0RGlyOiAnLi4vanMnLFxyXG4gICAgICAgIGVtcHR5T3V0RGlyOiB0cnVlLFxyXG4gICAgICAgIGxpYjoge1xyXG4gICAgICAgICAgICBlbnRyeTogcmVzb2x2ZShfX2Rpcm5hbWUsICcuL3NyYy9tYWluLnRzJyksXHJcbiAgICAgICAgICAgIGZvcm1hdHM6IFsnZXMnXSxcclxuICAgICAgICAgICAgbmFtZTogJ215bGliJyxcclxuICAgICAgICAgICAgZmlsZU5hbWU6ICgpID0+ICdtYWluLm1pbi5qcydcclxuICAgICAgICB9LFxyXG4gICAgICAgIHJvbGx1cE9wdGlvbnM6IHtcclxuICAgICAgICAgICAgZXh0ZXJuYWw6IFsnd2VidG9ycmVudCddLFxyXG4gICAgICAgICAgICBvdXRwdXQ6IHtcclxuICAgICAgICAgICAgICAgIGdsb2JhbHM6IHtcclxuICAgICAgICAgICAgICAgICAgICAnd2VidG9ycmVudCc6ICdXZWJUb3JyZW50J1xyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIGZvcm1hdDogJ2VzJyxcclxuICAgICAgICAgICAgICAgIGNvbXBhY3Q6IHRydWVcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgIH0gYXMgQnVpbGRPcHRpb25zLFxyXG4gICAgcmVzb2x2ZToge1xyXG4gICAgICAgIGFsaWFzOiB7XHJcbiAgICAgICAgICAgICdAJzogcmVzb2x2ZShfX2Rpcm5hbWUsICcuL3NyYycpXHJcbiAgICAgICAgfVxyXG4gICAgfSBhcyBSZXNvbHZlT3B0aW9uc1xyXG59KSJdLAogICJtYXBwaW5ncyI6ICI7QUFDQSxTQUFTLGVBQWU7QUFDeEIsU0FBUyxvQkFBa0Q7QUFGM0QsSUFBTSxtQ0FBbUM7QUFJekMsSUFBTyxzQkFBUSxhQUFhO0FBQUEsRUFDeEIsT0FBTztBQUFBLElBQ0gsUUFBUTtBQUFBLElBQ1IsYUFBYTtBQUFBLElBQ2IsS0FBSztBQUFBLE1BQ0QsT0FBTyxRQUFRLGtDQUFXLGVBQWU7QUFBQSxNQUN6QyxTQUFTLENBQUMsSUFBSTtBQUFBLE1BQ2QsTUFBTTtBQUFBLE1BQ04sVUFBVSxNQUFNO0FBQUEsSUFDcEI7QUFBQSxJQUNBLGVBQWU7QUFBQSxNQUNYLFVBQVUsQ0FBQyxZQUFZO0FBQUEsTUFDdkIsUUFBUTtBQUFBLFFBQ0osU0FBUztBQUFBLFVBQ0wsY0FBYztBQUFBLFFBQ2xCO0FBQUEsUUFDQSxRQUFRO0FBQUEsUUFDUixTQUFTO0FBQUEsTUFDYjtBQUFBLElBQ0o7QUFBQSxFQUNKO0FBQUEsRUFDQSxTQUFTO0FBQUEsSUFDTCxPQUFPO0FBQUEsTUFDSCxLQUFLLFFBQVEsa0NBQVcsT0FBTztBQUFBLElBQ25DO0FBQUEsRUFDSjtBQUNKLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==
