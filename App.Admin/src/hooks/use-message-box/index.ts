import { useOverlay, wdHookState } from "../index";
import type { DialogOptions, DialogResult } from "@wot-ui/ui/components/wd-dialog/types";

/** 确认弹窗配置 */
type MessageBoxConfirmOptions = Omit<DialogOptions, "beforeConfirm"> & {
	/** 确认前执行；返回 false 或抛错时阻止关闭，未返回值视为确认通过。 */
	beforeConfirm?: (value: string | number) => boolean | void | Promise<boolean | void>;
};

const defaultTitle = "温馨提示";

/**
 * 显示确认弹窗。
 *
 * @param options - 弹窗文本或 Wot UI 弹窗配置；未指定标题时使用“温馨提示”
 * @returns Wot UI 弹窗结果；取消或失败时 Promise 拒绝
 */
function confirm(options: MessageBoxConfirmOptions | string): Promise<DialogResult>;
/**
 * 使用文本和附加配置显示确认弹窗。
 *
 * @param message - 弹窗文本，优先于 options.msg
 * @param options - Wot UI 弹窗配置；未指定标题时使用“温馨提示”
 * @returns Wot UI 弹窗结果；取消或失败时 Promise 拒绝
 */
function confirm(message: string, options?: MessageBoxConfirmOptions): Promise<DialogResult>;
function confirm(messageOrOptions: MessageBoxConfirmOptions | string, options: MessageBoxConfirmOptions = {}): Promise<DialogResult> {
	const { beforeConfirm, ...baseOptions } =
		typeof messageOrOptions === "string"
			? { ...options, title: options.title ?? defaultTitle, msg: messageOrOptions }
			: { ...messageOrOptions, title: messageOrOptions.title ?? defaultTitle };

	const dialogOptions: DialogOptions = baseOptions;

	return new Promise<DialogResult>((resolve, reject) => {
		dialogOptions.beforeConfirm = async (value) => {
			if (!beforeConfirm) return true;

			useOverlay.show(0);
			wdHookState.wdDialogState.loading = true;

			try {
				return (await beforeConfirm(value)) !== false;
			} catch (error) {
				reject(error);
				return false;
			} finally {
				wdHookState.wdDialogState.loading = false;
				useOverlay.hide();
			}
		};

		wdHookState.wdDialog = {
			type: "confirm",
			options: dialogOptions,
			then: (res) => resolve(res),
			catch: (error) => reject(error),
		};
	});
}

/** 应用级弹窗入口，依赖当前可见页面布局处理请求 */
export const useMessageBox = {
	/**
	 * 显示消息弹窗。
	 *
	 * @param options - 弹窗文本或 Wot UI 弹窗配置；未指定标题时使用“温馨提示”
	 * @returns Wot UI 弹窗结果；取消或失败时 Promise 拒绝
	 */
	show(options: DialogOptions | string): Promise<DialogResult> {
		return new Promise<DialogResult>((resolve, reject) => {
			wdHookState.wdDialog = {
				type: "show",
				options: typeof options === "string" ? { title: defaultTitle, msg: options } : { ...options, title: options.title ?? defaultTitle },
				then: (res) => resolve(res),
				catch: (error) => reject(error),
			};
		});
	},
	/**
	 * 显示提示弹窗。
	 *
	 * @param options - 弹窗文本或 Wot UI 弹窗配置；未指定标题时使用“温馨提示”
	 * @returns Wot UI 弹窗结果；取消或失败时 Promise 拒绝
	 */
	alert(options: DialogOptions | string): Promise<DialogResult> {
		return new Promise<DialogResult>((resolve, reject) => {
			wdHookState.wdDialog = {
				type: "alert",
				options: typeof options === "string" ? { title: defaultTitle, msg: options } : { ...options, title: options.title ?? defaultTitle },
				then: (res) => resolve(res),
				catch: (error) => reject(error),
			};
		});
	},
	confirm,
	/**
	 * 显示输入弹窗。
	 *
	 * @param options - 弹窗文本或 Wot UI 弹窗配置；未指定标题时使用“温馨提示”
	 * @returns Wot UI 弹窗结果；取消或失败时 Promise 拒绝
	 */
	prompt(options: DialogOptions | string): Promise<DialogResult> {
		return new Promise<DialogResult>((resolve, reject) => {
			wdHookState.wdDialog = {
				type: "prompt",
				options: typeof options === "string" ? { title: defaultTitle, msg: options } : { ...options, title: options.title ?? defaultTitle },
				then: (res) => resolve(res),
				catch: (error) => reject(error),
			};
		});
	},
	/** 请求关闭当前弹窗。 */
	close(): void {
		wdHookState.wdDialog = {
			type: "close",
			options: undefined,
			then: undefined,
			catch: undefined,
		};
	},
};
