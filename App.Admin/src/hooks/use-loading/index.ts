import { wdHookState } from "../index";

/** 应用级加载提示，由页面布局渲染 */
export const useLoading = {
	/**
	 * 显示加载提示。
	 *
	 * @param options - 提示文字或加载配置；fullscreen 为 true 时使用全屏加载组件
	 */
	show(options?: { text?: string; fullscreen?: boolean } | string): void {
		if (typeof options === "string") {
			wdHookState.loading = {
				state: true,
				text: options,
				fullscreen: false,
			};
		} else {
			wdHookState.loading = {
				state: true,
				text: options?.text,
				fullscreen: options?.fullscreen,
			};
		}
	},
	/** 延迟 500 毫秒清除共享加载状态。 */
	hide(): void {
		if (wdHookState.loading?.state) {
			setTimeout(() => {
				wdHookState.loading = undefined;
			}, 500);
		}
	},
};
