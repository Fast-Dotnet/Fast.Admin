const storageKey = "fast_privacy_version";
// 需要重新征得同意时递增版本，使旧的同意记录失效。
const privacyVersion = "1";
const isApp = process.env.UNI_PLATFORM === "app" || process.env.UNI_PLATFORM === "app-harmony";

/** 管理 App 的隐私同意记录，弹窗由启动页处理。 */
export const usePrivacy = {
	/** 检查当前版本的同意记录；小程序和 Web 直接返回 true。 */
	hasAgreed(): boolean {
		return !isApp || uni.getStorageSync<string>(storageKey) === privacyVersion;
	},
	/** 用户主动同意后保存记录，并在 Android/iOS 通知 DCloud 引擎。 */
	agree(): void {
		if (!isApp) return;
		// #ifdef APP-PLUS
		plus.runtime.agreePrivacy();
		// #endif
		uni.setStorageSync(storageKey, privacyVersion);
	},
};
