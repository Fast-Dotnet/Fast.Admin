import * as Pinia from "pinia";
import { type App, createSSRApp } from "vue";
import AppVue from "./App.vue";
import { loadPlugins } from "./plugins";
import router from "./router";
import { loadPinia } from "./stores";
import "./styles/index.scss";
import "./components/index.scss";

export function createApp(): { app: App; Pinia: typeof Pinia } {
	const app = createSSRApp(AppVue);

	// 注册持久化存储
	loadPinia(app);

	loadPlugins(app);

	app.use(router);

	return {
		app,
		Pinia,
	};
}
