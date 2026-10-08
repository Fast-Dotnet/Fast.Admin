import type { App, Directive } from "vue";

/** 注册本地指令；当前指令表为空，不安装额外指令。 */
export function loadDirectives(app: App): void {
	const directivesList: Record<string, Directive> = {};

	Object.keys(directivesList).forEach((key) => {
		// 注册所有自定义指令
		app.directive(key, directivesList[key]);
	});
}
