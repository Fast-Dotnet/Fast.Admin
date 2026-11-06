import { configureLogger, configureStorage } from "@fast-china/utils";
import { isAxiosError } from "axios";
import dayjs from "dayjs";
import "dayjs/locale/zh-cn";
import { loadWotUi } from "./wot-ui";
import { loadZPaging } from "./z-paging";
import type { App, ComponentPublicInstance } from "vue";

/** 注册存储与 UI 默认配置；运行信息由 App.vue 在隐私同意后初始化。 */
export function loadPlugins(app: App): void {
	// 全局异常捕获
	app.config.errorHandler = (err, _instance: ComponentPublicInstance, _info: string) => {
		if (isAxiosError(err)) return;
		console.error(err);
		uni.showModal({
			title: "系统错误",
			content:
				err instanceof Error
					? err.message
					: typeof err === "object" && err !== null && "errMsg" in err && typeof err.errMsg === "string"
						? err.errMsg
						: JSON.stringify(err),
			showCancel: false,
		});
	};

	configureLogger({
		level: import.meta.env.DEV ? "debug" : "warn",
	});

	configureStorage({ prefix: "fast__", crypto: import.meta.env.VITE_STORAGE_CRYPTO === "true" });

	dayjs.locale("zh-cn");

	loadWotUi();
	loadZPaging();
}
