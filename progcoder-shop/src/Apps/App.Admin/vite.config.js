import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import rollupReplace from "@rollup/plugin-replace";
// https://vitejs.dev/config/
export default defineConfig(({ command }) => {
  // Only force __DEV__/NODE_ENV=development for `vite dev` - `vite build`
  // must keep Vite's own production NODE_ENV so React ships its production
  // (minified, no dev-mode checks) build, not the dev build.
  const isServe = command === "serve";

  return {
    resolve: {
      alias: [
        {
          find: "@",
          replacement: path.resolve(__dirname, "./src"),
        },
      ],
    },
    server: {
      port: 7002
    },
    plugins: [
      ...(isServe
        ? [
            rollupReplace({
              preventAssignment: true,
              values: {
                __DEV__: JSON.stringify(true),
                "process.env.NODE_ENV": JSON.stringify("development"),
              },
            }),
          ]
        : []),
      react(),
    ],
  };
});
