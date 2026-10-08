import { wdHookState } from "../index";

/** 应用级遮罩，由页面布局渲染 */
export const useOverlay = {
	/**
	 * 显示拦截交互的遮罩。
	 *
	 * @param transparent - 黑色遮罩的 alpha 值，范围为 0 到 1；默认 0，全透明但仍拦截交互
	 */
	show(transparent = 0): void {
		wdHookState.overlay = {
			state: true,
			transparent,
		};
	},
	/** 清除共享遮罩状态。 */
	hide(): void {
		if (wdHookState.overlay?.state) {
			wdHookState.overlay = undefined;
		}
	},
};
