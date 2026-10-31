import { checkAppUpdate } from "./app";
import { checkMiniProgramUpdate } from "./miniProgram";
import type { AppUpdateResult } from "./app";
import type { MiniProgramUpdateResult } from "./miniProgram";

/**
 * 检查当前平台的应用更新。
 *
 * @param manual - true 表示用户触发，false 表示系统触发，默认为 false
 * @returns 更新状态；required 表示仍需完成强制更新，failed 表示检测或下载失败
 * @remarks
 * 可按需 await；等待检测和下载提示，不等待用户确认或安装完成。
 * App 自动调用复用本次运行结果，手动调用重新检测；小程序由平台检测，仅复用监听结果。
 */
const checkUpdate = (manual = false): Promise<AppUpdateResult | MiniProgramUpdateResult> => {
	// #ifdef APP-PLUS || APP-HARMONY
	return checkAppUpdate(manual);
	// #endif

	// #ifdef MP
	return checkMiniProgramUpdate(manual);
	// #endif

	// #ifndef APP-PLUS || APP-HARMONY || MP
	if (manual) uni.showToast({ title: "当前为网页版，刷新页面即可加载最新版本", icon: "none" });
	return Promise.resolve("unsupported");
	// #endif
};

/** 应用更新入口 */
export const useUpdate = {
	checkUpdate,
};
