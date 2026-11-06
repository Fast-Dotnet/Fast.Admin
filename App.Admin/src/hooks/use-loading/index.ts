import { wdHookState } from "../index";

let activeLoadingCount = 0;
let hideTimer: ReturnType<typeof setTimeout> | undefined;

/** 应用级加载提示，由页面布局渲染 */
export const useLoading = {
	/**
	 * 显示加载提示。
	 *
	 * @param options - 提示文字或加载配置；fullscreen 为 true 时使用全屏加载组件
	 */
	show(options?: { text?: string; fullscreen?: boolean } | string): void {
		activeLoadingCount++;
		if (hideTimer !== undefined) {
			clearTimeout(hideTimer);
			hideTimer = undefined;
		}

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
	/** 默认延迟 500 毫秒关闭当前加载；弹窗前可立即关闭，不影响随后开始的新加载。 */
	hide(options?: { immediate?: boolean }): void {
		activeLoadingCount = Math.max(activeLoadingCount - 1, 0);
		if (activeLoadingCount > 0) return;

		if (options?.immediate) {
			if (hideTimer !== undefined) clearTimeout(hideTimer);
			hideTimer = undefined;
			wdHookState.loading = undefined;
			return;
		}

		if (hideTimer !== undefined || !wdHookState.loading?.state) return;

		hideTimer = setTimeout(() => {
			hideTimer = undefined;
			if (activeLoadingCount === 0) wdHookState.loading = undefined;
		}, 500);
	},
};
