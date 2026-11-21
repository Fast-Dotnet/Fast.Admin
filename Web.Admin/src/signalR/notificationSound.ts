import { logger } from "@fast-china/utils";
import notificationSoundUrl from "@/assets/audio/notification.wav";
import { useConfig } from "@/stores";

let audio: HTMLAudioElement;
let lastPlayedAt = 0;

/** 停止当前提示音。 */
export const stopNotificationSound = (): void => {
	if (!audio) return;
	audio.pause();
	audio.currentTime = 0;
};

/** 每批通知仅调用一次；浏览器拒绝自动播放时不影响文字通知，也不补播。 */
export const playNotificationSound = (): void => {
	if (!useConfig().layout.notificationSound) return;
	const now = Date.now();
	if (now - lastPlayedAt < 2000) return;
	lastPlayedAt = now;
	try {
		audio ??= new Audio(notificationSoundUrl);
		audio.volume = 0.6;
		audio.currentTime = 0;
		audio.play().catch((error: unknown) => {
			logger.debug("NotificationSound", "浏览器未播放通知提示音", error);
		});
	} catch (error) {
		logger.debug("NotificationSound", "通知提示音播放失败", error);
	}
};
