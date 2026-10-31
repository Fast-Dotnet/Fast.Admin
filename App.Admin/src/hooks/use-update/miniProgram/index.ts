import { logger } from "@fast-china/utils";

/** 小程序更新结果；ready 表示下载完成并已发起重启提示 */
export type MiniProgramUpdateResult = "latest" | "ready" | "restarting" | "failed" | "unsupported";
type UpdateState = "checking" | "downloading" | MiniProgramUpdateResult;

let updateState: UpdateState = "checking";
let updateManager: UniNamespace.UpdateManager | undefined;
let updatePromise: Promise<MiniProgramUpdateResult> | undefined;
let isPrompting = false;

const showUpdatePrompt = (): void => {
	const manager = updateManager;
	if (!manager || updateState !== "ready" || isPrompting) return;

	isPrompting = true;
	uni.showModal({
		title: "更新提示",
		content: "新版本已经准备好，需要重启后才能正常使用应用。",
		confirmText: "立即重启",
		showCancel: false,
		success({ confirm }) {
			if (!confirm || updateState !== "ready") return;

			updateState = "restarting";
			try {
				manager.applyUpdate();
			} catch (error: unknown) {
				updateState = "ready";
				logger.error("Update", "小程序重启更新失败", error);
				uni.showToast({
					title: "重启失败，请重新进入小程序",
					icon: "none",
				});
			}
		},
		fail(error) {
			logger.error("Update", "小程序更新提示弹窗失败", error);
		},
		complete() {
			isPrompting = false;
		},
	});
};

/**
 * 等待小程序更新结果，下载完成后自动提示重启。
 *
 * @param manual - 是否手动触发，用于显示本次启动的更新状态
 * @returns 无更新、下载就绪、失败或不支持时的状态，不等待用户确认重启
 * @remarks
 * 首次应在 App.onLaunch 中调用，避免遗漏平台回调；检查和下载期间共用同一个任务。
 * 平台在冷启动时自动检测，重复调用只能读取当前状态，不能主动重新请求版本。
 */
export const checkMiniProgramUpdate = (manual = false): Promise<MiniProgramUpdateResult> => {
	// #ifdef MP
	if (updatePromise === undefined) {
		if (!uni.canIUse("getUpdateManager")) {
			updateState = "unsupported";
			if (manual) {
				uni.showToast({
					title: "当前客户端不支持更新检测",
					icon: "none",
				});
			}
			return Promise.resolve("unsupported");
		}

		updatePromise = new Promise<MiniProgramUpdateResult>((resolve) => {
			const manager = uni.getUpdateManager();
			updateManager = manager;

			manager.onUpdateReady(() => {
				if (updateState !== "checking" && updateState !== "downloading") return;

				updateState = "ready";
				logger.debug("Update", "小程序新版本已下载完成");
				showUpdatePrompt();
				resolve("ready");
			});

			manager.onUpdateFailed((error: unknown) => {
				if (updateState !== "checking" && updateState !== "downloading") return;

				updateState = "failed";
				logger.error("Update", "小程序新版本下载失败", error);
				isPrompting = true;
				uni.showModal({
					title: "更新提示",
					content: "新版本下载失败，请检查网络后完全退出小程序，再重新进入。",
					showCancel: false,
					fail(modalError) {
						logger.error("Update", "小程序更新失败提示弹窗失败", modalError);
					},
					complete() {
						isPrompting = false;
					},
				});
				resolve("failed");
			});

			manager.onCheckForUpdate(({ hasUpdate }) => {
				logger.debug("Update", hasUpdate ? "小程序存在新版本" : "本次启动未发现新版本");

				// 避免迟到的检查回调覆盖下载完成或失败状态。
				if (updateState !== "checking") return;

				if (hasUpdate) {
					updateState = "downloading";
					// 有更新时继续等待下载结果。
					return;
				}

				updateState = "latest";
				resolve("latest");
			});
		}).catch((error: unknown): MiniProgramUpdateResult => {
			updateState = "failed";
			logger.error("Update", "小程序更新监听初始化失败", error);
			uni.showToast({ title: "更新检测失败，请重新进入小程序", icon: "none" });
			return "failed";
		});
	}

	if (updateState === "ready") {
		if (manual) showUpdatePrompt();
		return Promise.resolve("ready");
	}

	if (manual && !isPrompting && updateState !== "restarting") {
		const messages = {
			checking: "正在等待平台检查更新",
			downloading: "新版本正在下载，完成后会提示重启",
			latest: "本次启动未发现更新，完全退出后重新进入可再次检查",
			failed: "新版本下载失败，请完全退出后重新进入",
			unsupported: "当前客户端不支持更新检测",
		} satisfies Record<Exclude<UpdateState, "ready" | "restarting">, string>;

		uni.showToast({
			title: messages[updateState],
			icon: "none",
		});
	}

	if (updateState === "checking" || updateState === "downloading") {
		return updatePromise;
	}

	return Promise.resolve(updateState);
	// #endif

	// #ifndef MP
	if (manual) {
		uni.showToast({
			title: "当前平台不支持小程序更新检测",
			icon: "none",
		});
	}
	return Promise.resolve("unsupported");
	// #endif
};
