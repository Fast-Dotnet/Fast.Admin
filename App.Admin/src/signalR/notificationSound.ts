import { logger } from "@fast-china/utils";
import { useConfig } from "@/stores";

let isForeground = false;
let audio: UniApp.InnerAudioContext;
let lastPlayedAt = 0;

/** 释放提示音资源，不占用业务音频播放器。 */
export const stopNotificationSound = (): void => {
	if (!audio) return;
	const context = audio;
	audio = undefined;
	context.stop();
	context.destroy();
};

/** 移动端各平台仅在前台播放，后台通知不补播。 */
export const setNotificationSoundForeground = (value: boolean): void => {
	isForeground = value;
	if (!value) stopNotificationSound();
};

/** 每批通知仅播放一次；高频通知间隔至少两秒。 */
export const playNotificationSound = (): void => {
	if (!isForeground || !useConfig().layout.notificationSound) return;
	// #ifdef H5
	if (document.visibilityState !== "visible") return;
	// #endif
	const now = Date.now();
	if (now - lastPlayedAt < 2000) return;
	lastPlayedAt = now;
	try {
		stopNotificationSound();
		const context = uni.createInnerAudioContext();
		audio = context;
		audio.volume = 0.6;
		// #ifdef APP-PLUS
		// App 提示音混播并遵循静音模式；当前 Uni 类型声明尚未包含此原生属性。
		Object.assign(context, { sessionCategory: "ambient" });
		// #endif
		// #ifdef H5
		audio.src = `${import.meta.env.BASE_URL}static/audio/notification.wav`;
		// #endif
		// #ifndef H5
		audio.src = "/static/audio/notification.wav";
		// #endif
		audio.onEnded(() => {
			if (audio === context) stopNotificationSound();
		});
		audio.onError((error) => {
			logger.debug("NotificationSound", "通知提示音未播放", error);
			if (audio === context) stopNotificationSound();
		});
		audio.play();
	} catch (error) {
		logger.debug("NotificationSound", "通知提示音播放失败", error);
		stopNotificationSound();
	}
};
