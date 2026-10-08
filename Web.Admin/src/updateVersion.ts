import { ElMessage, ElMessageBox } from "element-plus";
import { logger } from "@fast-china/utils";

// 防止定时检测重复弹出版本更新确认框。
let versionUpdateDialogVisible = false;

/**
 * 获取当前部署环境中的最新版本号。
 *
 * 通过时间戳和 no-store 禁用缓存，避免读取到旧的 build-info.json。
 *
 * @returns 最新版本号；请求失败或文件格式无效时返回 null。
 */
const loadLatestVersion = async (): Promise<string> => {
	try {
		const publicPath = import.meta.env.VITE_PUBLIC_PATH.endsWith("/") ? import.meta.env.VITE_PUBLIC_PATH : `${import.meta.env.VITE_PUBLIC_PATH}/`;
		const response = await fetch(`${publicPath}build-info.json?_=${Date.now()}`, {
			cache: "no-store",
		});

		if (!response.ok) {
			throw new Error(`HTTP ${response.status}`);
		}

		const result = await response.json();

		// 校验 build-info.json 数据结构，避免直接使用不可信响应。
		if (typeof result !== "object" || result === null || !("version" in result) || typeof result.version !== "string") {
			throw new TypeError("build-info.json 格式无效");
		}

		return result.version;
	} catch (error) {
		// 检测失败不影响应用正常运行，等待下一次定时检测。
		logger.error("App", "版本更新检测失败", error);
		return null;
	}
};

/**
 * 比较当前版本与最新部署版本，并在版本发生变化时提示用户更新。
 *
 * @param currentVersion - 当前页面构建时使用的版本号
 */
const detectVersionUpdate = async (currentVersion: string): Promise<void> => {
	const latestVersion = await loadLatestVersion();

	// 未获取到版本、版本未变化或提示框已打开时无需继续处理。
	if (!latestVersion || latestVersion === currentVersion || versionUpdateDialogVisible) return;

	versionUpdateDialogVisible = true;

	try {
		await ElMessageBox.confirm(`发现新版本 ${latestVersion}，是否立即更新？`, {
			type: "warning",
			confirmButtonText: "更新",
			closeOnClickModal: false,
		});

		// 重新加载页面，获取最新的入口文件和静态资源。
		window.location.reload();
	} catch {
		// 用户取消更新后恢复检测状态，允许下一轮定时任务再次提示。
		versionUpdateDialogVisible = false;
		ElMessage.warning("您取消了更新，将在十分钟后再次进行提示！");
	}
};

/**
 * 启动版本更新检测。
 *
 * @remarks
 * 启动时立即检测一次，之后按照指定间隔持续检测；此方法不返回清理句柄，应在应用启动时调用一次。
 *
 * @param currentVersion - 当前页面构建时使用的版本号
 * @param delay - 检测间隔，单位为毫秒，默认十分钟
 */
export const checkVersionUpdate = (currentVersion: string, delay = 10 * 60 * 1000): void => {
	logger.log("App", `当前版本 ${currentVersion}`);
	detectVersionUpdate(currentVersion);

	window.setInterval(() => {
		detectVersionUpdate(currentVersion);
	}, delay);
};
