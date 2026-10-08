import { wdHookState } from "../index";
import type { DialogOptions, DialogResult } from "@wot-ui/ui/components/wd-dialog/types";

const defaultTitle = "温馨提示";

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
			wdHookState.wdMessageBox = {
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
			wdHookState.wdMessageBox = {
				type: "alert",
				options: typeof options === "string" ? { title: defaultTitle, msg: options } : { ...options, title: options.title ?? defaultTitle },
				then: (res) => resolve(res),
				catch: (error) => reject(error),
			};
		});
	},
	/**
	 * 显示确认弹窗。
	 *
	 * @param options - 弹窗文本或 Wot UI 弹窗配置；未指定标题时使用“温馨提示”
	 * @returns Wot UI 弹窗结果；取消或失败时 Promise 拒绝
	 */
	confirm(options: DialogOptions | string): Promise<DialogResult> {
		return new Promise<DialogResult>((resolve, reject) => {
			wdHookState.wdMessageBox = {
				type: "confirm",
				options: typeof options === "string" ? { title: defaultTitle, msg: options } : { ...options, title: options.title ?? defaultTitle },
				then: (res) => resolve(res),
				catch: (error) => reject(error),
			};
		});
	},
	/**
	 * 显示输入弹窗。
	 *
	 * @param options - 弹窗文本或 Wot UI 弹窗配置；未指定标题时使用“温馨提示”
	 * @returns Wot UI 弹窗结果；取消或失败时 Promise 拒绝
	 */
	prompt(options: DialogOptions | string): Promise<DialogResult> {
		return new Promise<DialogResult>((resolve, reject) => {
			wdHookState.wdMessageBox = {
				type: "prompt",
				options: typeof options === "string" ? { title: defaultTitle, msg: options } : { ...options, title: options.title ?? defaultTitle },
				then: (res) => resolve(res),
				catch: (error) => reject(error),
			};
		});
	},
	/** 请求关闭当前弹窗。 */
	close(): void {
		wdHookState.wdMessageBox = {
			type: "close",
			options: undefined,
			then: undefined,
			catch: undefined,
		};
	},
};
