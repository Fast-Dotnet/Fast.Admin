import { nextTick } from "vue";
import { ElNotification } from "element-plus";
import { configureInstallationIdentity, configureLogger, configureStorage, getOrCreateInstallationId } from "@fast-china/utils";
import { registerComponents } from "@/components";
import { loadFastAxios } from "./axios";
import { loadElementPlus } from "./element-plus";
import type { App, ComponentPublicInstance } from "vue";

/** 初始化应用日志、存储、请求与全局组件，并注册统一异常反馈。 */
export function loadPlugins(app: App): void {
	// 全局异常捕获
	app.config.errorHandler = (err, _instance: ComponentPublicInstance, _info: string) => {
		if (!err) return;
		const errorMap: Record<string, string> = {
			InternalError: "Javascript引擎内部错误",
			ReferenceError: "未找到对象",
			TypeError: "使用了错误的类型或对象",
			RangeError: "使用内置对象时，参数超范围",
			SyntaxError: "语法错误",
			EvalError: "错误的使用了Eval",
			URIError: "URI错误",
			AggregateError: "未知的多个错误",
			TimeoutError: "操作超时",
			NetworkError: "网络错误",
			OutOfMemoryError: "内存溢出",
			DOMException: "DOM 操作异常",
			SecurityError: "安全错误，可能涉及跨域或 CSP 限制",
			EventError: "事件处理错误",
		};
		const errorName = err instanceof Error ? err.name : undefined;
		if (err === "cancel") {
			console.warn("操作已取消");
		} else if (errorName === "AxiosError") {
			return;
		} else {
			const errorMessage = (errorName && errorMap[errorName]) || "未知错误";
			console.error(err);
			nextTick(() => {
				ElNotification({
					title: "系统错误",
					message: errorMessage,
					duration: 3000,
					position: "top-right",
				});
			});
		}
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
