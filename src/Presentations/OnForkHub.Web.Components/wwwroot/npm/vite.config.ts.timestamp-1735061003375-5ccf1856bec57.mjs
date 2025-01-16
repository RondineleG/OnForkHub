// vite.config.ts
import { resolve } from "path";
import { defineConfig } from "file:///C:/Dev/OnForkHub/src/Presentations/OnForkHub.Web.Components/wwwroot/npm/node_modules/vite/dist/node/index.js";
var __vite_injected_original_dirname = "C:\\Dev\\OnForkHub\\src\\Presentations\\OnForkHub.Web.Components\\wwwroot\\npm";
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
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJDOlxcXFxEZXZcXFxcT25Gb3JrSHViXFxcXHNyY1xcXFxQcmVzZW50YXRpb25zXFxcXE9uRm9ya0h1Yi5XZWIuQ29tcG9uZW50c1xcXFx3d3dyb290XFxcXG5wbVwiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9maWxlbmFtZSA9IFwiQzpcXFxcRGV2XFxcXE9uRm9ya0h1YlxcXFxzcmNcXFxcUHJlc2VudGF0aW9uc1xcXFxPbkZvcmtIdWIuV2ViLkNvbXBvbmVudHNcXFxcd3d3cm9vdFxcXFxucG1cXFxcdml0ZS5jb25maWcudHNcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfaW1wb3J0X21ldGFfdXJsID0gXCJmaWxlOi8vL0M6L0Rldi9PbkZvcmtIdWIvc3JjL1ByZXNlbnRhdGlvbnMvT25Gb3JrSHViLldlYi5Db21wb25lbnRzL3d3d3Jvb3QvbnBtL3ZpdGUuY29uZmlnLnRzXCI7aW1wb3J0IHtyZXNvbHZlfSBmcm9tICdwYXRoJ1xyXG5pbXBvcnQge0J1aWxkT3B0aW9ucywgZGVmaW5lQ29uZmlnLCBSZXNvbHZlT3B0aW9uc30gZnJvbSAndml0ZSdcclxuXHJcbmV4cG9ydCBkZWZhdWx0IGRlZmluZUNvbmZpZyh7XHJcbiAgICBidWlsZDoge1xyXG4gICAgICAgIG91dERpcjogJy4uL2pzJyxcclxuICAgICAgICBlbXB0eU91dERpcjogdHJ1ZSxcclxuICAgICAgICBsaWI6IHtcclxuICAgICAgICAgICAgZW50cnk6IHJlc29sdmUoX19kaXJuYW1lLCAnLi9zcmMvbWFpbi50cycpLFxyXG4gICAgICAgICAgICBmb3JtYXRzOiBbJ2VzJ10sXHJcbiAgICAgICAgICAgIG5hbWU6ICdteWxpYicsXHJcbiAgICAgICAgICAgIGZpbGVOYW1lOiAoKSA9PiAnbWFpbi5taW4uanMnXHJcbiAgICAgICAgfSxcclxuICAgICAgICByb2xsdXBPcHRpb25zOiB7XHJcbiAgICAgICAgICAgIGV4dGVybmFsOiBbJ3dlYnRvcnJlbnQnXSxcclxuICAgICAgICAgICAgb3V0cHV0OiB7XHJcbiAgICAgICAgICAgICAgICBnbG9iYWxzOiB7XHJcbiAgICAgICAgICAgICAgICAgICAgJ3dlYnRvcnJlbnQnOiAnV2ViVG9ycmVudCdcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICBmb3JtYXQ6ICdlcycsXHJcbiAgICAgICAgICAgICAgICBjb21wYWN0OiB0cnVlXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9IGFzIEJ1aWxkT3B0aW9ucyxcclxuICAgIHJlc29sdmU6IHtcclxuICAgICAgICBhbGlhczoge1xyXG4gICAgICAgICAgICAnQCc6IHJlc29sdmUoX19kaXJuYW1lLCAnLi9zcmMnKVxyXG4gICAgICAgIH1cclxuICAgIH0gYXMgUmVzb2x2ZU9wdGlvbnMsXHJcbiAgICBvcHRpbWl6ZURlcHM6IHtcclxuICAgICAgICBpbmNsdWRlOiBbJ3dlYnRvcnJlbnQnXVxyXG4gICAgfVxyXG59KTsiXSwKICAibWFwcGluZ3MiOiAiO0FBQXVaLFNBQVEsZUFBYztBQUM3YSxTQUFzQixvQkFBbUM7QUFEekQsSUFBTSxtQ0FBbUM7QUFHekMsSUFBTyxzQkFBUSxhQUFhO0FBQUEsRUFDeEIsT0FBTztBQUFBLElBQ0gsUUFBUTtBQUFBLElBQ1IsYUFBYTtBQUFBLElBQ2IsS0FBSztBQUFBLE1BQ0QsT0FBTyxRQUFRLGtDQUFXLGVBQWU7QUFBQSxNQUN6QyxTQUFTLENBQUMsSUFBSTtBQUFBLE1BQ2QsTUFBTTtBQUFBLE1BQ04sVUFBVSxNQUFNO0FBQUEsSUFDcEI7QUFBQSxJQUNBLGVBQWU7QUFBQSxNQUNYLFVBQVUsQ0FBQyxZQUFZO0FBQUEsTUFDdkIsUUFBUTtBQUFBLFFBQ0osU0FBUztBQUFBLFVBQ0wsY0FBYztBQUFBLFFBQ2xCO0FBQUEsUUFDQSxRQUFRO0FBQUEsUUFDUixTQUFTO0FBQUEsTUFDYjtBQUFBLElBQ0o7QUFBQSxFQUNKO0FBQUEsRUFDQSxTQUFTO0FBQUEsSUFDTCxPQUFPO0FBQUEsTUFDSCxLQUFLLFFBQVEsa0NBQVcsT0FBTztBQUFBLElBQ25DO0FBQUEsRUFDSjtBQUFBLEVBQ0EsY0FBYztBQUFBLElBQ1YsU0FBUyxDQUFDLFlBQVk7QUFBQSxFQUMxQjtBQUNKLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==