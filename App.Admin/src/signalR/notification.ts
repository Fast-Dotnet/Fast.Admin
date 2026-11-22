import { shallowRef } from "vue";

interface QueuedNotification {
	id: number;
	/** 消息类型 */
	type: "info" | "warning" | "danger";
	/** 通知内容 */
	messages: string[];
	/** 图标 */
	prefix: string;
}

/** 通知类型对应的展示配置 */
const notificationOptions: Record<
	string,
	{
		type: "info" | "warning" | "danger";
		prefix: string;
	}
> = {
	info: { type: "info", prefix: "info-circle" },
	warning: { type: "warning", prefix: "warn-bold" },
	error: { type: "danger", prefix: "error-circle" },
} as const;

/** 当前展示的通知；布局切换时复用同一份状态。 */
export const activeNotification = shallowRef<QueuedNotification>(null);

/** 待展示通知队列 */
const pendingNotifications: QueuedNotification[] = [];

/** 通知序列号，用于生成唯一标识，确保 NoticeBar 切换时重新初始化 */
let sequence = 0;

/** 展示下一组通知 */
const showNextNotification = () => {
	activeNotification.value = pendingNotifications.shift() ?? null;
};

/** 接收并排队展示通知，避免高频推送相互覆盖。 */
export const enqueueNotification = (data: { type: "info" | "warning" | "error"; messages: string[] }): void => {
	// 上限只约束待展示队列，不影响当前可见的通知。
	if (pendingNotifications.length >= 50) pendingNotifications.shift();

	const options = notificationOptions[data.type];

	pendingNotifications.push({
		id: ++sequence,
		messages: data.messages,
		...options,
	});

	if (!activeNotification.value) showNextNotification();
};

/** 当前通知滚动结束或被手动关闭时，展示下一组通知 */
export const dismissNotification = (): void => {
	if (activeNotification.value) showNextNotification();
};

/** 退出登录和租户切换时清理通知，避免消息残留 */
export const clearNotifications = (): void => {
	pendingNotifications.length = 0;
	activeNotification.value = null;
};
