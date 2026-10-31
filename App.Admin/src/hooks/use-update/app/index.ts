import { logger } from "@fast-china/utils";
import { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import { appApi } from "@/api/services/Center/app";
import type { CheckAppUpdatePackageOutput } from "@/api/services/Center/app/models/CheckAppUpdatePackageOutput";

/** App 更新结果；required 表示仍需完成强制更新，store 表示已发起商店更新提示 */
export type AppUpdateResult = "latest" | "ready" | "required" | "store" | "installing" | "restarting" | "failed" | "unsupported";

// #ifdef APP-PLUS || APP-HARMONY
let checkPromise: Promise<AppUpdateResult> | undefined;
let lastResult: AppUpdateResult | undefined;
let readyUpdate:
	| { package: CheckAppUpdatePackageOutput; filePath: string }
	| { package: CheckAppUpdatePackageOutput; store: "apple"; storeUrl: string }
	| { package: CheckAppUpdatePackageOutput; store: "huawei" }
	| undefined;
let applyState: "ready" | "installing" | "restarting" = "ready";
let isPrompting = false;
let isUpdateRequired = false;

const showUpdateError = (message: string, error: unknown): void => {
	logger.error("Update", message, error);
	uni.showToast({ title: message, icon: "none" });
};

const showUpdatePrompt = (): void => {
	const update = readyUpdate;
	if (!update || isPrompting || applyState !== "ready") return;

	const isWgt = update.package.packageType === ApplicationPackageTypeEnum.Wgt;
	const isStore = "store" in update;
	const isHarmony = isStore && update.store === "huawei";
	const storeName = isHarmony ? "华为应用市场" : "App Store";
	const message = isStore
		? `已发布，请${isHarmony ? "自行" : ""}前往${storeName}更新。`
		: isWgt
			? "已下载完成，重启后生效。"
			: "已下载完成，是否立即安装？";
	isPrompting = true;
	uni.showModal({
		title: "更新提示",
		content: `新版本 ${update.package.versionName || update.package.versionCode} ${message}\n${update.package.updateContent || ""}`,
		confirmText: isHarmony ? "我知道了" : isStore ? "前往更新" : isWgt ? "立即重启" : "立即安装",
		cancelText: "稍后更新",
		showCancel: !isHarmony && !update.package.isForceUpdate,
		success({ confirm }) {
			if (!confirm || applyState !== "ready") return;

			if ("store" in update) {
				// #ifdef APP-PLUS
				try {
					if (update.store === "apple") {
						plus.runtime.openURL(update.storeUrl, (error) => showUpdateError("无法打开 App Store，请稍后重试", error));
					}
				} catch (error: unknown) {
					showUpdateError("无法打开应用商店，请稍后重试", error);
				}
				// #endif
				return;
			}

			// #ifdef APP-PLUS
			if (!isWgt) {
				// APK 不触发成功回调；不锁定安装状态，用户取消系统安装后仍可重试。
				try {
					plus.runtime.install(update.filePath, {}, undefined, (error) => showUpdateError("无法打开安装程序，请重试", error));
				} catch (error: unknown) {
					showUpdateError("无法打开安装程序，请重试", error);
				}
				return;
			}
			applyState = "installing";
			new Promise<void>((resolve, reject) => {
				// 保留运行时的 AppID、版本等安装校验，不强制覆盖。
				plus.runtime.install(
					update.filePath,
					{ force: false },
					() => resolve(),
					(error) => reject(new Error(error.message || "安装失败"))
				);
			})
				.then(() => {
					applyState = "restarting";
					plus.runtime.restart();
				})
				.catch((error: unknown) => {
					if (applyState === "restarting") {
						showUpdateError("更新已安装，请重新打开应用", error);
						return;
					}
					applyState = "ready";
					showUpdateError("应用更新安装失败，请重试", error);
				});
			// #endif
		},
		fail(error) {
			showUpdateError("无法显示更新提示，请重试", error);
		},
		complete() {
			isPrompting = false;
		},
	});
};
// #endif

// #ifdef APP-PLUS
const downloadPackage = (update: CheckAppUpdatePackageOutput): Promise<string> => {
	if (!update.downloadUrl || !/^https?:\/\//i.test(update.downloadUrl) || !(Number(update.fileSizeKb) > 0)) {
		return Promise.reject(new Error("更新包下载信息不完整"));
	}
	return new Promise<string>((resolve, reject) => {
		const suffix = update.packageType === ApplicationPackageTypeEnum.Wgt ? "wgt" : "apk";
		// 显式保留扩展名，避免匿名下载地址和临时文件名导致运行时无法识别安装包。
		plus.downloader
			.createDownload(
				update.downloadUrl,
				{ filename: `_doc/update/${update.versionCode}.${suffix}`, timeout: 60, retry: 1 },
				(task, status) => {
					if ((status !== 200 && status !== 206) || !task.filename || !task.downloadedSize) {
						reject(new Error(`更新包下载失败（HTTP ${status}）`));
						return;
					}
					// 后端 FileSizeKb 是字节数除以 1024 后向上取整。
					if (Math.ceil(task.downloadedSize / 1024) !== Number(update.fileSizeKb)) {
						reject(new Error("更新包大小不匹配，请重新下载"));
						return;
					}
					resolve(task.filename);
				}
			)
			.start();
	});
};
// #endif

/**
 * 检查 App 版本，下载更新包或提示前往应用商店。
 *
 * @param manual - 是否手动触发，无更新时显示提示
 * @returns 强制更新返回 required，下载就绪返回 ready，商店更新返回 store；失败返回 failed
 * @remarks
 * 请求实例初始化后调用。并发调用共用任务，自动调用复用本次运行结果，手动调用重新请求。
 * 不等待用户确认或安装完成；关闭弹窗不代表强制更新已完成。
 * Android 支持 APK，Android/iOS 支持 WGT；iOS 整包跳转商店，原生鸿蒙仅提示用户自行前往应用市场。
 * 已知强制更新失败时仍返回 required，调用方据此阻止跳过更新。
 */
export const checkAppUpdate = (manual = false): Promise<AppUpdateResult> => {
	// #ifdef APP-PLUS || APP-HARMONY
	if (checkPromise !== undefined) return checkPromise;
	if (applyState !== "ready") return Promise.resolve(applyState);
	if (isPrompting) return Promise.resolve(isUpdateRequired ? "required" : readyUpdate && "store" in readyUpdate ? "store" : "ready");
	if (!manual && lastResult !== undefined) return Promise.resolve(lastResult);

	checkPromise = (async (): Promise<AppUpdateResult> => {
		const nativeVersionCode = Number(uni.getAppBaseInfo().appVersionCode);
		const resourceVersionCode = await new Promise<number>((resolve) => {
			// #ifdef APP-PLUS
			plus.runtime.getProperty(plus.runtime.appid, (info) => resolve(Number(info.versionCode)));
			// #endif
			// #ifdef APP-HARMONY
			resolve(nativeVersionCode);
			// #endif
		});
		if (
			!Number.isSafeInteger(nativeVersionCode) ||
			nativeVersionCode < 1 ||
			!Number.isSafeInteger(resourceVersionCode) ||
			resourceVersionCode < 1
		) {
			throw new Error("无法读取应用版本号，请使用正式包或自定义基座验证");
		}

		const result = await appApi.checkAppUpdate({ nativeVersionCode, resourceVersionCode }).catch((): undefined => {
			// 请求异常已经由统一请求层提示，避免重复弹出错误，也不记录包含认证信息的请求对象。
			logger.warn("Update", "应用更新检测请求失败");
			return undefined;
		});
		if (!result) return isUpdateRequired ? "required" : "failed";
		if (result.hasUpdate === false) {
			readyUpdate = undefined;
			isUpdateRequired = false;
			if (manual) uni.showToast({ title: "当前已是最新版本", icon: "none" });
			return "latest";
		}
		if (result.hasUpdate !== true) throw new Error("更新检测结果无效");

		const update = result.package;
		// 下载、校验或商店跳转失败都不能解除已知的强制更新。
		if (update?.isForceUpdate) isUpdateRequired = true;
		if (!update || !Number.isSafeInteger(update.versionCode) || update.versionCode <= Math.max(nativeVersionCode, resourceVersionCode)) {
			throw new Error("更新包版本信息无效");
		}
		if (update.packageType !== ApplicationPackageTypeEnum.Wgt && update.packageType !== ApplicationPackageTypeEnum.FullPackage) {
			throw new Error("不支持的更新包类型");
		}
		// #ifdef APP-HARMONY
		if (process.env.UNI_PLATFORM === "app-harmony") {
			if (update.packageType !== ApplicationPackageTypeEnum.FullPackage) throw new Error("原生鸿蒙暂不支持 WGT 热更新");
			readyUpdate = { package: update, store: "huawei" };
			isUpdateRequired = update.isForceUpdate === true;
			showUpdatePrompt();
			return isUpdateRequired ? "required" : "store";
		}
		// #endif
		// #ifdef APP-PLUS
		if (update.packageType === ApplicationPackageTypeEnum.FullPackage) {
			const platform = uni.getDeviceInfo().platform;
			if (platform === "ios") {
				const appleAppId = import.meta.env.VITE_APPLE_STORE_APP_ID?.trim();
				if (!appleAppId || !/^[1-9]\d*$/.test(appleAppId)) throw new Error("请配置有效的 VITE_APPLE_STORE_APP_ID");
				readyUpdate = { package: update, store: "apple", storeUrl: `https://apps.apple.com/cn/app/id${appleAppId}` };
				isUpdateRequired = update.isForceUpdate === true;
				showUpdatePrompt();
				return isUpdateRequired ? "required" : "store";
			}
			if (platform !== "android") return "unsupported";
		}

		const cachedUpdate = readyUpdate;
		const filePath =
			cachedUpdate !== undefined &&
			"filePath" in cachedUpdate &&
			cachedUpdate.package.downloadUrl === update.downloadUrl &&
			cachedUpdate.package.fileSha256 === update.fileSha256 &&
			cachedUpdate.package.versionCode === update.versionCode
				? cachedUpdate.filePath
				: await downloadPackage(update);
		readyUpdate = { package: update, filePath };
		isUpdateRequired = update.isForceUpdate === true;
		showUpdatePrompt();
		return isUpdateRequired ? "required" : "ready";
		// #endif
	})()
		.catch((error: unknown): AppUpdateResult => {
			showUpdateError("应用更新失败，请稍后重试", error);
			return isUpdateRequired ? "required" : "failed";
		})
		.then((result) => {
			lastResult = result;
			// 弹窗确认可能先于检测任务结束，优先返回正在安装或重启的状态。
			return applyState !== "ready" ? applyState : result;
		})
		.finally(() => {
			checkPromise = undefined;
		});
	return checkPromise;
	// #endif

	// #ifndef APP-PLUS || APP-HARMONY
	if (manual) uni.showToast({ title: "当前平台不支持 App 更新", icon: "none" });
	return Promise.resolve("unsupported");
	// #endif
};
