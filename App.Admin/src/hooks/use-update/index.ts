import { logger } from "@fast-china/utils";
import { useLoading } from "../use-loading";
import { useMessageBox } from "../use-message-box";

/** APP 当前版本信息 */
export interface AppVersionInfo {
	/** APPID */
	appId: string;
	/** 整包版本名称 */
	version: string;
	/** 整包版本号 */
	versionCode: string;
	/** 当前资源版本名称 */
	resourceVersion: string;
}

/** APP 更新选项 */
export interface AppUpdateOptions {
	/** 更新包下载地址 */
	url: string;
	/** 新版本名称 */
	version?: string;
	/** 更新说明 */
	note?: string;
	/** 是否强制更新 */
	force?: boolean;
}

/** APP 更新信息 */
export interface AppUpdateInfo extends AppUpdateOptions {
	/** 更新类型，app 为整包更新，wgt 为资源热更新 */
	type: "app" | "wgt";
}

/** 查询更新信息的方法；返回 null 或 undefined 表示无需更新 */
export type AppUpdateChecker = (versionInfo: AppVersionInfo) => Promise<AppUpdateInfo | null | undefined>;

const getErrorMessage = (error: unknown): string => {
	if (error instanceof Error) return error.message;
	if (typeof error === "object" && error) {
		const message = Reflect.get(error, "message") ?? Reflect.get(error, "errMsg");
		if (typeof message === "string") return message;
	}
	return typeof error === "string" && error ? error : "未知错误";
};

const confirmUpdate = async (updateInfo: AppUpdateOptions): Promise<boolean> => {
	const versionText = updateInfo.version ? ` v${updateInfo.version}` : "";
	const msg = updateInfo.note || `发现新版本${versionText}，是否立即更新？`;

	try {
		if (updateInfo.force) {
			await useMessageBox.alert({
				title: "更新提示",
				msg,
				confirmButtonText: "立即更新",
				closeOnClickModal: false,
			});
		} else {
			await useMessageBox.confirm({
				title: "更新提示",
				msg,
				confirmButtonText: "立即更新",
				cancelButtonText: "暂不更新",
			});
		}
		return true;
	} catch {
		return false;
	}
};

const checkMiniProgramUpdate = (): Promise<boolean> => {
	// #ifdef MP
	return new Promise<boolean>((resolve, reject) => {
		const updateManager = uni.getUpdateManager();

		updateManager.onUpdateReady(() => {
			logger.debug("Update", "小程序新版本已下载完成");
			resolve(true);
			useMessageBox
				.alert({
					title: "更新提示",
					msg: "新版本已经准备好，需要重启后才能正常使用应用。",
					confirmButtonText: "立即重启",
					closeOnClickModal: false,
				})
				.then(() =>
					//调用 applyUpdate 应用新版本并重启
					updateManager.applyUpdate()
				);
		});

		updateManager.onUpdateFailed((error) => {
			const message = getErrorMessage(error);
			logger.error("Update", "小程序新版本下载失败", error);
			useMessageBox.alert({
				title: "更新失败",
				msg: "新版本下载失败，请删除小程序后重新打开。",
				closeOnClickModal: false,
			});
			reject(new Error(message));
		});

		updateManager.onCheckForUpdate(({ hasUpdate }) => {
			logger.debug("Update", hasUpdate ? "小程序存在新版本" : "小程序已是最新版本");
			if (!hasUpdate) resolve(false);
		});
	});
	// #endif

	// #ifndef MP
	logger.warn("Update", "当前平台不支持小程序更新检测");
	return Promise.resolve(false);
	// #endif
};

const updateApp = async (options: AppUpdateOptions | string): Promise<boolean> => {
	// #ifdef APP-PLUS
	const updateInfo = typeof options === "string" ? { url: options } : options;
	if (!(await confirmUpdate(updateInfo))) return false;

	const url = updateInfo.url.trim();
	if (!url) throw new Error("APP 更新地址不能为空");

	plus.runtime.openURL(url, (error) => {
		logger.error("Update", "无法打开 APP 更新地址", error);
		useMessageBox
			.alert({
				title: "更新失败",
				msg: getErrorMessage(error),
			})
			.catch((dialogError: unknown) => logger.warn("Update", "关闭 APP 更新失败提示", dialogError));
	});
	return true;
	// #endif

	// #ifndef APP-PLUS
	logger.warn("Update", "当前平台不支持 APP 整包更新");
	return false;
	// #endif
};

const hotUpdateApp = async (options: AppUpdateOptions | string): Promise<boolean> => {
	// #ifdef APP-PLUS
	const updateInfo = typeof options === "string" ? { url: options } : options;
	if (!(await confirmUpdate(updateInfo))) return false;

	const url = updateInfo.url.trim();
	if (!url) throw new Error("APP 热更新地址不能为空");

	useLoading.show("下载更新中...");
	try {
		const { statusCode, tempFilePath } = await uni.downloadFile({ url });
		if (statusCode !== 200 || !tempFilePath) throw new Error(`资源文件下载失败（HTTP ${statusCode}）`);

		await new Promise<void>((resolve, reject) => {
			plus.runtime.install(
				tempFilePath,
				{ force: false },
				() => resolve(),
				(error) => reject(new Error(getErrorMessage(error)))
			);
		});

		logger.debug("Update", "APP 热更新安装完成");
		await useMessageBox.alert({
			title: "更新提示",
			msg: "新版本已经准备好，需要重启后才能正常使用应用。",
			confirmButtonText: "立即重启",
			closeOnClickModal: false,
		});
		plus.runtime.restart();
		return true;
	} catch (error) {
		logger.error("Update", "APP 热更新失败", error);
		throw error;
	} finally {
		useLoading.hide();
	}
	// #endif

	// #ifndef APP-PLUS
	logger.warn("Update", "当前平台不支持 APP 热更新");
	return false;
	// #endif
};

const checkAppUpdate = async (checker: AppUpdateChecker): Promise<boolean> => {
	// #ifdef APP-PLUS
	const widgetInfo = await new Promise<PlusRuntimeWidgetInfo>((resolve) => {
		plus.runtime.getProperty(plus.runtime.appid, resolve);
	});
	const updateInfo = await checker({
		appId: widgetInfo.appid || plus.runtime.appid,
		version: plus.runtime.version,
		versionCode: plus.runtime.versionCode,
		resourceVersion: widgetInfo.version || plus.runtime.version,
	});

	if (!updateInfo) {
		logger.debug("Update", "APP 已是最新版本");
		return false;
	}

	logger.debug("Update", "APP 存在新版本", updateInfo);
	return updateInfo.type === "wgt" ? hotUpdateApp(updateInfo) : updateApp(updateInfo);
	// #endif

	// #ifndef APP-PLUS
	logger.warn("Update", "当前平台不支持 APP 更新检测");
	return false;
	// #endif
};

/** 应用更新 Hook */
export const useUpdate = {
	/**
	 * 注册小程序更新检测并在资源就绪后提示重启。
	 *
	 * @returns 资源就绪为 true，无更新或非小程序平台为 false；下载失败时 Promise 拒绝
	 */
	checkMiniProgramUpdate,
	/**
	 * 使用调用方的检测方法查询版本，并按更新类型执行整包或资源更新。
	 *
	 * @param checker - 接收当前版本信息并返回可用更新的检测方法
	 * @returns 是否进入对应更新流程；无更新、取消或非 App 平台时为 false
	 */
	checkAppUpdate,
	/**
	 * 用户确认后通过系统打开整包下载地址，不在本方法内安装整包。
	 *
	 * @param options - 下载地址或更新配置
	 * @returns 是否已调用系统打开地址，不代表下载或安装完成
	 */
	updateApp,
	/**
	 * 用户确认后下载并安装 WGT 资源包，再提示重启应用。
	 *
	 * @param options - 资源包地址或更新配置
	 * @returns 是否完成安装并调用重启；取消或非 App 平台时为 false
	 * @throws 下载、安装或重启前确认失败时继续传播异常。
	 */
	hotUpdateApp,
};
