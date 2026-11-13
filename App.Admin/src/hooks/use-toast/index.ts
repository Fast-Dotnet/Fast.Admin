import { wdHookState } from "../index";
import type { ToastOptions } from "@wot-ui/ui/components/wd-toast/types";

/** 应用级轻提示入口，由当前可见页面的布局消费 */
export const useToast = {
	/**
	 * 显示提示。
	 *
	 * @param options - 提示文本或 Wot UI 轻提示配置
	 */
	show(options: ToastOptions | string): void {
		wdHookState.wdToast = {
			type: "show",
			options,
		};
	},
	/**
	 * 显示成功提示。
	 *
	 * @param options - 提示文本或 Wot UI 轻提示配置
	 */
	success(options: ToastOptions | string): void {
		wdHookState.wdToast = {
			type: "success",
			options,
		};
	},
	/**
	 * 显示错误提示。
	 *
	 * @param options - 提示文本或 Wot UI 轻提示配置
	 */
	error(options: ToastOptions | string): void {
		wdHookState.wdToast = {
			type: "error",
			options,
		};
	},
	/**
	 * 显示常规提示。
	 *
	 * @param options - 提示文本或 Wot UI 轻提示配置
	 */
	info(options: ToastOptions | string): void {
		wdHookState.wdToast = {
			type: "info",
			options,
		};
	},
	/**
	 * 显示警告提示。
	 *
	 * @param options - 提示文本或 Wot UI 轻提示配置
	 */
	warning(options: ToastOptions | string): void {
		wdHookState.wdToast = {
			type: "warning",
			options,
		};
	},
	/**
	 * 显示加载提示。
	 *
	 * @param options - 提示文本或 Wot UI 轻提示配置
	 */
	loading(options: ToastOptions | string): void {
		wdHookState.wdToast = {
			type: "loading",
			options,
		};
	},
	/** 关闭当前轻提示。 */
	close(): void {
		wdHookState.wdToast = {
			type: "close",
			options: undefined,
		};
	},
};
