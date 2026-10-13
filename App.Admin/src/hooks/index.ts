import { reactive } from "vue";
import { withDefineType } from "@fast-china/utils";
import type { Dialog, DialogOptions, DialogResult } from "@wot-ui/ui/components/wd-dialog/types";
import type { NotifyProps } from "@wot-ui/ui/components/wd-notify/types";
import type { Toast, ToastOptions } from "@wot-ui/ui/components/wd-toast/types";

export * from "./use-audio";
export * from "./use-form-schema";
export * from "./use-loading";
export * from "./use-message-box";
export * from "./use-notify";
export * from "./use-overlay";
export * from "./use-paging";
export * from "./use-toast";
export * from "./use-update";

/**
 * 页面布局与应用级提示方法之间的共享状态。
 *
 * @remarks
 * 通知、轻提示和弹窗采用单槽请求，由可见页面布局消费后清空，不提供队列。
 * 加载与遮罩状态由布局直接渲染，调用方负责关闭。
 */
export const wdHookState = reactive({
	/** 加载状态 */
	loading: {
		/**
		 * 是否显示
		 * @defaultValue false
		 */
		state: false,
		/**
		 * 加载提示文本
		 * @defaultValue "加载中..."
		 */
		text: "加载中...",
		/**
		 * 是否使用全屏加载组件
		 * @defaultValue false
		 */
		fullscreen: false,
	},
	/** 遮罩层 */
	overlay: {
		/**
		 * 是否显示
		 * @defaultValue false
		 */
		state: false,
		/**
		 * 黑色遮罩的 alpha 值，0 为全透明，1 为不透明
		 * @defaultValue 0
		 */
		transparent: 0,
	},
	/** Notify 通知 */
	wdNotify: {
		/** 通知操作类型 */
		type: withDefineType<"showNotify" | "closeNotify">(),
		/** 通知配置或通知内容 */
		options: withDefineType<NotifyProps | string>(),
	},
	/** Toast 消息 */
	wdToast: {
		/** Toast 操作类型 */
		type: withDefineType<keyof Toast>(),
		/** Toast 配置或消息内容 */
		options: withDefineType<ToastOptions | string>(),
	},
	/** Dialog 消息弹窗 */
	wdMessageBox: {
		/** 弹窗操作类型 */
		type: withDefineType<keyof Dialog>(),
		/** 弹窗配置 */
		options: withDefineType<DialogOptions>(),
		/** 弹窗操作成功后的回调 */
		then: withDefineType<(res?: DialogResult) => void>(),
		/** 弹窗操作失败或取消后的回调 */
		catch: withDefineType<(error?: unknown) => void>(),
	},
});
