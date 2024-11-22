// vite.config.ts
import { resolve } from "path";
import { defineConfig } from "file:///C:/Users/Micro/source/repos/OnForkHub.Web.Components/src/OnForkHub.Web.Components/wwwroot/npm/node_modules/vite/dist/node/index.js";
var __vite_injected_original_dirname = "C:\\Users\\Micro\\source\\repos\\OnForkHub.Web.Components\\src\\OnForkHub.Web.Components\\wwwroot\\npm";
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
  },
  optimizeDeps: {
    include: ["webtorrent"]
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJDOlxcXFxVc2Vyc1xcXFxNaWNyb1xcXFxzb3VyY2VcXFxccmVwb3NcXFxcQmxhem9yVmlkZW9QbGF5ZXJcXFxcc3JjXFxcXEJsYXpvclZpZGVvUGxheWVyXFxcXHd3d3Jvb3RcXFxcbnBtXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ZpbGVuYW1lID0gXCJDOlxcXFxVc2Vyc1xcXFxNaWNyb1xcXFxzb3VyY2VcXFxccmVwb3NcXFxcQmxhem9yVmlkZW9QbGF5ZXJcXFxcc3JjXFxcXEJsYXpvclZpZGVvUGxheWVyXFxcXHd3d3Jvb3RcXFxcbnBtXFxcXHZpdGUuY29uZmlnLnRzXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ltcG9ydF9tZXRhX3VybCA9IFwiZmlsZTovLy9DOi9Vc2Vycy9NaWNyby9zb3VyY2UvcmVwb3MvQmxhem9yVmlkZW9QbGF5ZXIvc3JjL0JsYXpvclZpZGVvUGxheWVyL3d3d3Jvb3QvbnBtL3ZpdGUuY29uZmlnLnRzXCI7aW1wb3J0IHsgcmVzb2x2ZSB9IGZyb20gJ3BhdGgnXHJcbmltcG9ydCB7IGRlZmluZUNvbmZpZywgQnVpbGRPcHRpb25zLCBSZXNvbHZlT3B0aW9ucyB9IGZyb20gJ3ZpdGUnXHJcblxyXG5leHBvcnQgZGVmYXVsdCBkZWZpbmVDb25maWcoe1xyXG4gICAgYnVpbGQ6IHtcclxuICAgICAgICBvdXREaXI6ICcuLi9qcycsXHJcbiAgICAgICAgZW1wdHlPdXREaXI6IHRydWUsXHJcbiAgICAgICAgbGliOiB7XHJcbiAgICAgICAgICAgIGVudHJ5OiByZXNvbHZlKF9fZGlybmFtZSwgJy4vc3JjL21haW4udHMnKSxcclxuICAgICAgICAgICAgZm9ybWF0czogWydlcyddLFxyXG4gICAgICAgICAgICBuYW1lOiAnbXlsaWInLFxyXG4gICAgICAgICAgICBmaWxlTmFtZTogKCkgPT4gJ21haW4ubWluLmpzJ1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcm9sbHVwT3B0aW9uczoge1xyXG4gICAgICAgICAgICBleHRlcm5hbDogWyd3ZWJ0b3JyZW50J10sXHJcbiAgICAgICAgICAgIG91dHB1dDoge1xyXG4gICAgICAgICAgICAgICAgZ2xvYmFsczoge1xyXG4gICAgICAgICAgICAgICAgICAgICd3ZWJ0b3JyZW50JzogJ1dlYlRvcnJlbnQnXHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICAgICAgZm9ybWF0OiAnZXMnLFxyXG4gICAgICAgICAgICAgICAgY29tcGFjdDogdHJ1ZVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfSBhcyBCdWlsZE9wdGlvbnMsXHJcbiAgICByZXNvbHZlOiB7XHJcbiAgICAgICAgYWxpYXM6IHtcclxuICAgICAgICAgICAgJ0AnOiByZXNvbHZlKF9fZGlybmFtZSwgJy4vc3JjJylcclxuICAgICAgICB9XHJcbiAgICB9IGFzIFJlc29sdmVPcHRpb25zLFxyXG4gICAgb3B0aW1pemVEZXBzOiB7XHJcbiAgICAgICAgaW5jbHVkZTogWyd3ZWJ0b3JyZW50J11cclxuICAgIH1cclxufSk7Il0sCiAgIm1hcHBpbmdzIjogIjtBQUFtYixTQUFTLGVBQWU7QUFDM2MsU0FBUyxvQkFBa0Q7QUFEM0QsSUFBTSxtQ0FBbUM7QUFHekMsSUFBTyxzQkFBUSxhQUFhO0FBQUEsRUFDeEIsT0FBTztBQUFBLElBQ0gsUUFBUTtBQUFBLElBQ1IsYUFBYTtBQUFBLElBQ2IsS0FBSztBQUFBLE1BQ0QsT0FBTyxRQUFRLGtDQUFXLGVBQWU7QUFBQSxNQUN6QyxTQUFTLENBQUMsSUFBSTtBQUFBLE1BQ2QsTUFBTTtBQUFBLE1BQ04sVUFBVSxNQUFNO0FBQUEsSUFDcEI7QUFBQSxJQUNBLGVBQWU7QUFBQSxNQUNYLFVBQVUsQ0FBQyxZQUFZO0FBQUEsTUFDdkIsUUFBUTtBQUFBLFFBQ0osU0FBUztBQUFBLFVBQ0wsY0FBYztBQUFBLFFBQ2xCO0FBQUEsUUFDQSxRQUFRO0FBQUEsUUFDUixTQUFTO0FBQUEsTUFDYjtBQUFBLElBQ0o7QUFBQSxFQUNKO0FBQUEsRUFDQSxTQUFTO0FBQUEsSUFDTCxPQUFPO0FBQUEsTUFDSCxLQUFLLFFBQVEsa0NBQVcsT0FBTztBQUFBLElBQ25DO0FBQUEsRUFDSjtBQUFBLEVBQ0EsY0FBYztBQUFBLElBQ1YsU0FBUyxDQUFDLFlBQVk7QUFBQSxFQUMxQjtBQUNKLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==
