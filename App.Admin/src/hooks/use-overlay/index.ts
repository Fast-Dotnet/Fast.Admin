import { wdHookState } from "../index";

let activeOverlayCount = 0;

/** 应用级遮罩，由页面布局渲染 */
export const useOverlay = {
	/**
	 * 显示拦截交互的遮罩。
	 *
	 * @param transparent - 黑色遮罩的 alpha 值，范围为 0 到 1；默认 0，全透明但仍拦截交互
	 */
	show(transparent = 0): void {
		activeOverlayCount++;
		wdHookState.overlay = {
			state: true,
			transparent,
		};
	},
	/** 清除共享遮罩状态。 */
	hide(): void {
		activeOverlayCount = Math.max(activeOverlayCount - 1, 0);
		if (activeOverlayCount > 0) return;

		if (wdHookState.overlay?.state) {
			wdHookState.overlay = undefined;
		}
	},
};
