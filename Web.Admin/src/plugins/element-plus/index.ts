import { dayjs } from "element-plus";
import FastElementPlus from "fast-element-plus";
import type { App } from "vue";
if (import.meta.env.DEV) {
	await import("dayjs/locale/zh-cn");
}

/** 设置日期语言，并完整安装 Fast Element Plus 组件。 */
export function loadElementPlus(app: App): void {
	dayjs.locale("zh-cn");

	// 完整注册 Fast Element Plus 组件。
	app.use(FastElementPlus);
}
