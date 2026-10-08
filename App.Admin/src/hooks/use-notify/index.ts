import { wdHookState } from "../index";
import type { NotifyProps } from "@wot-ui/ui/components/wd-notify/types";

/** 应用级通知入口，由当前可见页面的布局消费 */
export const useNotify = {
	/**
	 * 显示消息通知。
	 *
	 * @param options - 通知文本或 Wot UI 通知配置
	 */
	showNotify(options: NotifyProps | string): void {
		wdHookState.wdNotify = {
			type: "showNotify",
			options,
		};
	},
	/** 关闭当前消息通知。 */
	closeNotify(): void {
		wdHookState.wdNotify = {
			type: "closeNotify",
			options: undefined,
		};
	},
};
