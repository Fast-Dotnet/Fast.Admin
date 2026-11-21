import { ElLoading, ElMessageBox, ElNotification } from "element-plus";
import { useFastAxios } from "@fast-china/axios";
import { installationIdentity, logger, toQueryString, withDefineType } from "@fast-china/utils";
import { HttpTransportType, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { authApi } from "@/api/services/Auth/auth";
import { useUserInfo } from "@/stores";
import type { HubConnection } from "@microsoft/signalr";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";

/** SignalR 对象 */
let connection: HubConnection = null;
/** 是否保持实时连接，主动退出后不再自动恢复 */
let shouldConnect = false;
/** 是否已显示重连提示弹窗，避免多个连接失败通知重复弹窗 */
let reconnectDialogOpen = false;
/** 当前连接轮次，防止旧连接的异步回调关闭新连接的遮罩 */
let connectionVersion = 0;

let loadingInstance = withDefineType<ReturnType<typeof ElLoading.service>>(null);
let reLoadingInstance = withDefineType<ReturnType<typeof ElLoading.service>>(null);

/** 连接失败时提示重新连接，合并同时发生的失败通知。 */
const showReconnectDialog = async (): Promise<void> => {
	if (reconnectDialogOpen || !shouldConnect) return;
	reconnectDialogOpen = true;
	try {
		await ElMessageBox.alert("连接失败，请检查网络后重新连接", {
			type: "warning",
			confirmButtonText: "重新连接",
			showClose: false,
		});
	} catch (error) {
		logger.error("WebSocket", error);
		return;
	} finally {
		reconnectDialogOpen = false;
	}

	const userInfoStore = useUserInfo();
	if (!shouldConnect || !userInfoStore.token || !userInfoStore.asyncRouterGen) return;
	if (connection && connection.state !== HubConnectionState.Disconnected) return;

	// eslint-disable-next-line no-use-before-define -- 确认重连后调用下方定义的初始化方法，回调执行时该方法已完成初始化
	await initWebSocket();
};

/** 首次创建 SignalR 连接、注册服务端事件并启动连接；失败时记录日志 */
const initWebSocket = async (): Promise<void> => {
	shouldConnect = true;
	try {
		if (connection) {
			if (connection.state === HubConnectionState.Disconnected) {
				loadingInstance = ElLoading.service({
					fullscreen: true,
					lock: true,
					text: "服务连接中...",
					background: "rgba(0, 0, 0, 0.7)",
				});
				await connection.start();
			}
			return;
		}

		const fastAxios = useFastAxios();
		const userInfoStore = useUserInfo();
		const url = `${fastAxios.baseUrl}/hubs/chatHub`;
		const params = {
			"Fast-Origin": String(fastAxios.headers["Fast-Origin"]),
			"Fast-Device-Type": String(fastAxios.headers["Fast-Device-Type"]),
			"Fast-Device-Id": installationIdentity.deviceId,
		};
		connection = new HubConnectionBuilder()
			.withUrl(`${url}?${toQueryString(params)}`, {
				// 跳过协商阶段
				skipNegotiation: true,
				// 传输方式
				transport: HttpTransportType.WebSockets,
				// 日志级别
				logger: import.meta.env.DEV ? LogLevel.Information : LogLevel.Error,
				// 记录消息内容
				logMessageContent: true,
				accessTokenFactory: async () => {
					await authApi.refreshToken();
					return userInfoStore.getToken().token;
				},
			})
			// 日志级别
			.configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Error)
			.withAutomaticReconnect({
				// 每 5 秒重试，连续失败超过 1 分钟后不再安排下一次重试
				nextRetryDelayInMilliseconds: ({ elapsedMilliseconds }) => {
					if (!shouldConnect || !userInfoStore.token || !userInfoStore.asyncRouterGen) {
						return null;
					}
					return elapsedMilliseconds >= 60_000 ? null : 5000;
				},
			})
			.build();

		// 心跳检测 15s
		connection.keepAliveIntervalInMilliseconds = 15 * 1000;
		// 超时时间1m
		connection.serverTimeoutInMilliseconds = 60 * 1000;

		// 判断 WebSocket 是否已经连接
		if (connection.state !== HubConnectionState.Connected && connection.state !== HubConnectionState.Connecting) {
			// 重连期间显示遮罩，阻止继续操作。
			connection.onreconnecting(() => {
				connectionVersion++;
				ElNotification({
					title: "温馨提示",
					message: "服务器已断线...",
					type: "error",
					position: "top-right",
				});
				logger.warn("WebSocket", "服务器已断线...");
				reLoadingInstance ||= ElLoading.service({
					fullscreen: true,
					lock: true,
					text: "断线重连中...",
					background: "rgba(0, 0, 0, 0.7)",
				});
			});

			// 传输连接恢复后，继续等待业务登录成功。
			connection.onreconnected(() => {
				logger.debug("WebSocket", "服务重连成功，等待系统登录...");
			});
		}

		// 连接成功监听
		connection.on("ConnectSuccess", async () => {
			const currentVersion = ++connectionVersion;

			loadingInstance = ElLoading.service({
				fullscreen: true,
				lock: true,
				text: "系统连接中...",
				background: "rgba(0, 0, 0, 0.7)",
			});

			try {
				// WebSocket 登录
				await connection.invoke("Login");

				if (currentVersion !== connectionVersion || !shouldConnect || connection.state !== HubConnectionState.Connected) {
					return;
				}

				loadingInstance?.close();
				loadingInstance = null;

				reLoadingInstance?.close();
				reLoadingInstance = null;

				ElNotification({
					message: `系统连接成功`,
					type: "success",
					duration: 1000,
				});
				logger.debug("WebSocket", "系统连接成功...", userInfoStore.employeeName);
			} catch (error) {
				logger.error("WebSocket", error);
				if (currentVersion !== connectionVersion || !shouldConnect) return;
				// 传输连接已建立但业务登录失败，先停止连接，允许用户重新尝试。
				try {
					await connection.stop();
				} catch (stopError) {
					logger.error("WebSocket", stopError);
				}
				await showReconnectDialog();
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
				ElMessageBox.alert(message, { type: "warning" });
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

		// 实时通知
		connection.on("ReceiveNotification", (data: { type: "info" | "warning" | "error"; messages: string[] }) => {
			if (!shouldConnect || connection.state !== HubConnectionState.Connected) return;

			logger.log("WebSocket", "实时通知", data);
			for (const message of data.messages) {
				ElNotification({
					title: "消息通知",
					message,
					type: data.type,
					position: "top-right",
				});
			}
		});

		// 连接结束时使旧回调失效；主动退出清理遮罩，异常断开保留遮罩并提示重连。
		connection.onclose((error) => {
			connectionVersion++;
			// 主动退出或认证失效后解除遮罩，允许进入登录页面。
			if (!shouldConnect) {
				loadingInstance?.close();
				loadingInstance = null;

				reLoadingInstance?.close();
				reLoadingInstance = null;
				return;
			}
			if (error) {
				logger.error("WebSocket", error);
			}
			showReconnectDialog();
		});

		loadingInstance = ElLoading.service({
			fullscreen: true,
			lock: true,
			text: "服务连接中...",
			background: "rgba(0, 0, 0, 0.7)",
		});
		// 完成事件注册后再启动连接。
		await connection.start();
	} catch (error) {
		// 连接失败时保留遮罩，禁止继续操作
		logger.error("WebSocket", error);
		await showReconnectDialog();
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
const tryConnectWebSocket = async (): Promise<void> => {
	const userInfoStore = useUserInfo();
	if (!shouldConnect || !userInfoStore.token || !userInfoStore.asyncRouterGen) return;
	// Connected 状态是否已经失效，继续由现有心跳和超时机制判断。
	if (connection && connection.state !== HubConnectionState.Disconnected) return;
	await initWebSocket();
};

export { connection as signalR, initWebSocket, closeWebSocket, tryConnectWebSocket };
