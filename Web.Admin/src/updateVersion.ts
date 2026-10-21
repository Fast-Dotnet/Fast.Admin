import { ElMessage, ElMessageBox } from "element-plus";
import { logger } from "@fast-china/utils";

// 防止定时检测重复弹出版本更新确认框。
let versionUpdateDialogVisible = false;
const cancelledVersionReminderTimes = new Map<string, number>();
const visibilityCheckInterval = 10 * 60 * 1000;
const reminderCooldown = 60 * 60 * 1000;

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

	// 请求期间页面可能已隐藏，此时不弹出更新提示。
	if (document.hidden || !latestVersion || latestVersion === currentVersion || versionUpdateDialogVisible) return;

	if (Date.now() < (cancelledVersionReminderTimes.get(latestVersion) ?? 0)) return;
	cancelledVersionReminderTimes.delete(latestVersion);

	versionUpdateDialogVisible = true;

	try {
		await ElMessageBox.confirm(`发现新版本 ${latestVersion}，是否立即更新？`, {
			type: "warning",
			confirmButtonText: "更新",
			closeOnClickModal: false,
		});

		// 重新加载页面，获取最新的入口文件和静态资源。
		window.location.reload();
	} catch (error) {
		versionUpdateDialogVisible = false;
		if (error !== "cancel" && error !== "close") throw error;

		// 取消和关闭均从操作发生时计时，其他版本不受此提醒间隔影响。
		cancelledVersionReminderTimes.set(latestVersion, Date.now() + reminderCooldown);
		ElMessage.warning("您取消了更新，同一版本将在一小时内不再提醒！");
	}
};

/**
 * 启动版本更新检测。
 *
 * @remarks
 * 页面可见时启动即检测，之后按照指定间隔检测；页面隐藏时跳过，恢复可见且距上次检测超过十分钟时补查。
 * 用户取消后，同一版本一小时内不再提醒。此方法不返回清理句柄，应在应用启动时调用一次。
 *
 * @param currentVersion - 当前页面构建时使用的版本号
 * @param delay - 检测间隔，单位为毫秒，默认三十分钟
 */
export const checkVersionUpdate = (currentVersion: string, delay = 30 * 60 * 1000): void => {
	let lastCheckedAt = 0;
	let isChecking = false;

	const check = async (): Promise<void> => {
		if (document.hidden || isChecking || versionUpdateDialogVisible) return;

		isChecking = true;
		lastCheckedAt = Date.now();
		try {
			await detectVersionUpdate(currentVersion);
		} catch (error) {
			logger.error("App", "版本更新检测失败", error);
		} finally {
			isChecking = false;
		}
	};

	logger.log("App", `当前版本 ${currentVersion}`);
	check();

	window.setInterval(() => {
		check();
	}, delay);

	document.addEventListener("visibilitychange", () => {
		if (!document.hidden && Date.now() - lastCheckedAt > visibilityCheckInterval) {
			check();
		}
	});
};
