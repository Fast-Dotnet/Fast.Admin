import { useFastAxios } from "@fast-china/axios";
import { installationIdentity, logger, toQueryString } from "@fast-china/utils";
import { SignalRSocketState, createSignalRSocket } from "./uniSignalR";
import type { SignalRSocket } from "./uniSignalR";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";
import type { useUserInfo } from "@/stores";

/** SignalR 对象 */
let connection: SignalRSocket = null;
/** 是否保持实时连接，主动退出后不再自动恢复 */
let shouldConnect = false;

/**
 * 首次创建 SignalR 连接、注册服务端事件并启动连接；失败时记录日志
 * 这里应该是全局唯一的 uni.showLoading 和 uni.hideLoading 调用地方，其他地方请使用 useLoading 或者页面组件，避免冲突
 */
const initWebSocket = async (userInfoStore: ReturnType<typeof useUserInfo>): Promise<void> => {
	shouldConnect = true;

	try {
		if (connection) {
			if (connection.state === SignalRSocketState.Disconnected) {
				uni.showLoading({
					title: "服务连接中...",
					mask: true,
				});
				await connection.start();
			}
			return;
		}

		const fastAxios = useFastAxios();
		let baseUrl = fastAxios.baseUrl;
		// #ifdef H5
		baseUrl = new URL(baseUrl || "/", window.location.origin).href;
		// #endif
		const url = `${baseUrl.replace(/\/$/, "")}/hubs/chatHub`;
		const params = {
			"Fast-Origin": String(fastAxios.headers["Fast-Origin"]),
			"Fast-Device-Type": String(fastAxios.headers["Fast-Device-Type"]),
			"Fast-Device-Id": installationIdentity.deviceId,
		};
		connection = createSignalRSocket(`${url}?${toQueryString(params)}`, {
			accessTokenFactory: () => userInfoStore.getToken().token,
			automaticReconnect: {
				// 每5秒重连一次
				nextRetryDelayInMilliseconds: () => 5000,
			},
			// 心跳检测 15s
			keepAliveIntervalInMilliseconds: 15 * 1000,
			// 超时时间1m
			serverTimeoutInMilliseconds: 60 * 1000,
			// 单次 Hub 调用超时时间 15s
			invocationTimeoutInMilliseconds: 15 * 1000,
		});

		// 判断 WebSocket 是否已经连接
		if (connection.state !== SignalRSocketState.Connected && connection.state !== SignalRSocketState.Connecting) {
			// 重连期间显示遮罩，阻止继续操作。
			connection.onreconnecting(() => {
				logger.warn("WebSocket", "服务器已断线...");
				uni.showLoading({
					title: "断线重连中...",
					mask: true,
				});
			});

			// 重连成功后关闭遮罩并提示用户。
			connection.onreconnected(() => {
				uni.hideLoading();
				logger.debug("WebSocket", "服务重连成功...");
			});
		}

		// 连接成功监听
		connection.on("ConnectSuccess", async () => {
			uni.hideLoading();
			logger.debug("WebSocket", "服务连接成功...", userInfoStore.employeeName);
			try {
				uni.showLoading({
					title: "系统连接中...",
					mask: true,
				});
				// WebSocket 登录
				await connection.invoke("Login");
				logger.debug("WebSocket", "系统连接成功...");
			} finally {
				uni.hideLoading();
			}
		});

		// 登录失败监听
		connection.on("LoginFail", async (message: string) => {
			shouldConnect = false;
			try {
				// 认证失败时直接停止连接及自动重连，无需再调用 Hub Logout。
				await connection.stop();
			} catch (error) {
				logger.error("WebSocket", error);
			} finally {
				userInfoStore.logoutClear();
				uni.showModal({
					title: "温馨提示",
					content: message,
					showCancel: false,
				});
			}
		});

		// 其他地方登录监听
		connection.on("ElsewhereLogin", async (data: TenantOnlineUserModel) => {
			logger.warn("WebSocket", "其他地方登录", data);

			const message = `账号于【${data.lastLoginTime}】在【${data.lastLoginOS} ${data.lastLoginBrowser}(${
				data.lastLoginIp
			})】设备登录，如非本人操作，请及时修改密码！`;

			// 退出登录
			await userInfoStore.logout({
				type: 2,
				message,
			});
		});

		// 强制下线监听
		connection.on(
			"ForceOffline",
			async (data: { isAdmin: boolean; nickName: string; employeeNo: string; offlineTime: string; message: string }) => {
				logger.warn("WebSocket", "强制下线", data);

				let message = "您已被";

				if (data.isAdmin) {
					message += `管理员【${data.nickName}（${data.employeeNo}）】`;
				} else {
					message += `用户【${data.nickName}（${data.employeeNo}）】`;
				}

				message += `于【${data.offlineTime}】被强制下线！`;

				if (data.message) {
					message += `\r\n原因：${data.message}`;
				}

				// 退出登录
				await userInfoStore.logout({
					type: 1,
					message,
				});
			}
		);

		uni.showLoading({
			title: "服务连接中...",
			mask: true,
		});
		// 完成事件注册后再启动连接。
		await connection.start();
	} catch (error) {
		logger.error("WebSocket", error);
	}
};

/** 尝试通知服务端退出登录后停止连接；停止失败时向调用方抛出异常 */
const closeWebSocket = async (): Promise<void> => {
	shouldConnect = false;
	if (connection) {
		try {
			// WebSocket 退出登录
			await connection.invoke("Logout");
		} catch (error) {
			logger.error("WebSocket", error);
		}
		await connection.stop();
	}
};

/** 页面恢复前台时尝试连接，不重复启动正在连接或自动重连的实例 */
const tryConnectWebSocket = async (userInfoStore: ReturnType<typeof useUserInfo>): Promise<void> => {
	if (!shouldConnect || !userInfoStore.token || !userInfoStore.hasUserInfo) return;
	// Connected 状态是否已经失效，继续由现有心跳和超时机制判断。
	if (connection && connection.state !== SignalRSocketState.Disconnected) return;
	await initWebSocket(userInfoStore);
};

export { connection as signalR, initWebSocket, closeWebSocket, tryConnectWebSocket };
