import { createApp } from "vue";
import App from "./App.vue";
import { loadDirectives } from "./directives";
import { loadPlugins } from "./plugins";
import router from "./router";
import { loadPinia, useApp } from "./stores";
import { checkVersionUpdate } from "./updateVersion";
import type { Component } from "vue";
if (import.meta.env.DEV) {
	await import("element-plus/dist/index.css");
	await import("element-plus/theme-chalk/dark/css-vars.css");
	await import("fast-element-plus/index.css");
	await import("vue-json-pretty/lib/styles.css");
}
await import("./styles/index.scss");

checkVersionUpdate(import.meta.env.VITE_APP_VERSION);

const app = createApp(App as Component);

// 注册持久化存储
loadPinia(app);

/** 加载插件 */
loadPlugins(app);

/** Launch */
await useApp().launch();

/** 加载路由 */
app.use(router);

/** 加载自定义指令 */
loadDirectives(app);

app.mount("#app");
