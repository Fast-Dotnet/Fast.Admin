import { nextTick } from "vue";
import { ElNotification } from "element-plus";
import { configureInstallationIdentity, configureLogger, configureStorage, getOrCreateInstallationId } from "@fast-china/utils";
import { isAxiosError } from "axios";
import { registerComponents } from "@/components";
import { loadFastAxios } from "./axios";
import { loadElementPlus } from "./element-plus";
import type { App, ComponentPublicInstance } from "vue";

/** 初始化应用日志、存储、请求与全局组件，并注册统一异常反馈。 */
export function loadPlugins(app: App): void {
	// 全局异常捕获
	app.config.errorHandler = (err, _instance: ComponentPublicInstance, _info: string) => {
		if (isAxiosError(err)) return;
		console.error(err);
		nextTick(() => {
			ElNotification({
				type: "error",
				title: "系统错误",
				// eslint-disable-next-line @typescript-eslint/no-base-to-string
				message: err instanceof Error ? err.message : String(err),
				duration: 3000,
				position: "top-right",
			});
		});
	};

	configureLogger({
		level: import.meta.env.DEV ? "debug" : "warn",
	});

	configureStorage({ prefix: "fast__", crypto: import.meta.env.VITE_STORAGE_CRYPTO === "true" });

	configureInstallationIdentity();
	getOrCreateInstallationId();

	loadElementPlus(app);

	loadFastAxios();

	/** 注册本地全局组件 */
	registerComponents(app);
}
