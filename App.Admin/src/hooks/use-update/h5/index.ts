import { logger } from "@fast-china/utils";
import { useMessageBox } from "../../use-message-box";

declare const __FA_H5_BUILD_ID__: string;

/** H5 更新检测结果；ready 表示发现新构建并已提示用户。 */
export type H5UpdateResult = "latest" | "ready" | "failed";

type H5BuildInfo = {
	version: string;
	buildId: string;
};

const checkInterval = 30 * 60 * 1000;
const visibilityCheckInterval = 10 * 60 * 1000;
const reminderCooldown = 60 * 60 * 1000;
const cancelledReminderTimes = new Map<string, number>();

let checkPromise: Promise<H5UpdateResult> | undefined;
let isPrompting = false;
let autoCheckStarted = false;
let lastCheckedAt = 0;

/** 直接读取当前部署构建信息，禁止缓存及单页路由回退到 index.html 的误判。 */
const loadLatestBuild = async (): Promise<H5BuildInfo> => {
	const basePath = import.meta.env.BASE_URL.endsWith("/") ? import.meta.env.BASE_URL : `${import.meta.env.BASE_URL}/`;
	const response = await fetch(`${basePath}build-info.json?_=${Date.now()}`, { cache: "no-store" });
	if (!response.ok) throw new Error(`更新信息请求失败（HTTP ${response.status}）`);

	const data: unknown = await response.json();
	if (
		typeof data !== "object" ||
		data === null ||
		!("version" in data) ||
		!("buildId" in data) ||
		typeof data.version !== "string" ||
		typeof data.buildId !== "string" ||
		!data.version ||
		!data.buildId
	) {
		throw new TypeError("build-info.json 格式无效");
	}
	return { version: data.version, buildId: data.buildId };
};

const isCancelAction = (error: unknown): boolean =>
	error === "cancel" ||
	error === "close" ||
	(typeof error === "object" && error !== null && "action" in error && (error.action === "cancel" || error.action === "close"));

/** 对比构建标识并提示刷新；用户取消后同一构建一小时内不重复提醒。 */
const detectUpdate = async (manual: boolean): Promise<H5UpdateResult> => {
	const latest = await loadLatestBuild();
	if (latest.buildId === __FA_H5_BUILD_ID__) {
		if (manual) uni.showToast({ title: "当前已是最新版本", icon: "none" });
		return "latest";
	}

	if (isPrompting || (!manual && Date.now() < (cancelledReminderTimes.get(latest.buildId) ?? 0))) return "ready";

	isPrompting = true;
	try {
		await useMessageBox.confirm(`发现新版本 ${latest.version}，是否立即更新？`, {
			title: "版本更新",
			confirmButtonText: "立即更新",
			cancelButtonText: "稍后更新",
			closeOnClickModal: false,
		});
		// 不清除登录和业务缓存，直接重载最新入口及带哈希的静态资源。
		window.location.reload();
		return "ready";
	} catch (error: unknown) {
		if (!isCancelAction(error)) throw error;
		cancelledReminderTimes.set(latest.buildId, Date.now() + reminderCooldown);
		return "ready";
	} finally {
		isPrompting = false;
	}
};

/** 手动检测始终执行；自动检测失败仅记录日志，不阻断页面使用。 */
export const checkH5Update = (manual = false): Promise<H5UpdateResult> => {
	if (import.meta.env.DEV) {
		if (manual) uni.showToast({ title: "开发环境无需检查更新", icon: "none" });
		return Promise.resolve("latest");
	}
	if (document.hidden) return Promise.resolve("latest");
	if (checkPromise !== undefined) return checkPromise;

	checkPromise = detectUpdate(manual)
		.catch((error: unknown): H5UpdateResult => {
			logger.warn("Update", "H5 版本更新检测失败", error);
			if (manual) uni.showToast({ title: "检查更新失败，请稍后重试", icon: "none" });
			return "failed";
		})
		.finally(() => {
			checkPromise = undefined;
		});
	return checkPromise;
};

/** 首次进入 H5 页面时检测，并按 Web.Admin 的频率注册定时及返回前台补查。 */
export const startH5AutoCheck = (): void => {
	if (autoCheckStarted || import.meta.env.DEV) return;
	autoCheckStarted = true;

	const check = () => {
		if (document.hidden || isPrompting) return;
		lastCheckedAt = Date.now();
		checkH5Update().catch((error: unknown) => {
			logger.warn("Update", "H5 自动版本检测失败", error);
		});
	};

	check();
	window.setInterval(check, checkInterval);
	document.addEventListener("visibilitychange", () => {
		if (!document.hidden && Date.now() - lastCheckedAt > visibilityCheckInterval) check();
	});
	window.addEventListener("pageshow", () => {
		if (!document.hidden && Date.now() - lastCheckedAt > visibilityCheckInterval) check();
	});
};
