import { ElLoading, ElMessage, ElMessageBox, ElNotification, dayjs } from "element-plus";
import { useFastAxios } from "@fast-china/axios";
import { installationIdentity, logger, toQueryString, withDefineType } from "@fast-china/utils";
import { HttpTransportType, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { useApp, useUserInfo } from "@/stores";
import type { HubConnection } from "@microsoft/signalr";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";

/**
 * SignalR 对象
 */
let connection: HubConnection = null;

let loadingInstance = withDefineType<ReturnType<typeof ElLoading.service>>(null);
let reLoadingInstance = withDefineType<ReturnType<typeof ElLoading.service>>(null);

/**
 * 初始化 WebSocket
 */
const initWebSocket = async (): Promise<void> => {
	try {
		const fastAxios = useFastAxios();
		const appStore = useApp();
		const userInfoStore = useUserInfo();
		const url = `${fastAxios.baseUrl}${appStore.webSocketUrl}`;
		const params = {
			"Fast-Origin": import.meta.env.DEV ? import.meta.env.VITE_APP_ORIGIN || window.location.host : window.location.host,
			"Fast-Device-Type": Object.entries(AppEnvironmentEnum).find(([, value]) => value === AppEnvironmentEnum.Web)?.[0],
			"Fast-Device-Id": installationIdentity.deviceId,
		};
		// 判断是否存在对象
		if (!connection) {
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
					accessTokenFactory: () => {
						return userInfoStore.getToken().token;
					},
				})
				// 日志级别
				.configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Error)
				.withAutomaticReconnect({
					nextRetryDelayInMilliseconds: () => {
						// 每5秒重连一次
						return 5000;
					},
				})
				.build();

			// 心跳检测 15s
			connection.keepAliveIntervalInMilliseconds = 15 * 1000;
			// 超时时间1m
			connection.serverTimeoutInMilliseconds = 60 * 1000;

			// 判断 WebSocket 是否已经连接
			if (connection.state !== HubConnectionState.Connected && connection.state !== HubConnectionState.Connecting) {
				/**
				 * 重连中
				 */
				connection.onreconnecting(() => {
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

				/**
				 * 重连成功
				 */
				connection.onreconnected(() => {
					reLoadingInstance?.close();
					ElMessage.success("服务重连成功");
					logger.debug("WebSocket", "服务重连成功...");
				});
			}

			loadingInstance = ElLoading.service({
				fullscreen: true,
				lock: true,
				text: "服务连接中...",
				background: "rgba(0, 0, 0, 0.7)",
			});

			// 连接成功监听
			connection.on("ConnectSuccess", async () => {
				loadingInstance?.close();
				ElNotification({
					message: `服务连接成功`,
					type: "success",
					duration: 1000,
				});
				logger.debug("WebSocket", "服务连接成功...", userInfoStore.employeeName);

				try {
					loadingInstance = ElLoading.service({
						fullscreen: true,
						lock: true,
						text: "系统连接中...",
						background: "rgba(0, 0, 0, 0.7)",
					});
					// WebSocket 登录
					await connection.invoke("Login");
					ElNotification({
						message: `系统连接成功`,
						type: "success",
						duration: 1000,
					});
					logger.debug("WebSocket", "系统连接成功...");
				} finally {
					loadingInstance?.close();
				}
			});

			// 登录失败监听
			connection.on("LoginFail", (message: string) => {
				userInfoStore.logoutClear();
				ElMessageBox.alert(message, {
					type: "warning",
				});
			});

			// 其他地方登录监听
			connection.on("ElsewhereLogin", async (data: TenantOnlineUserModel) => {
				logger.warn("WebSocket", "其他地方登录", data);

				const message = `账号于【${dayjs(data.lastLoginTime).format("YYYY-MM-DD HH:mm:ss")}】在【${data.lastLoginOS} ${data.lastLoginBrowser}(${
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

					message += `于【${dayjs(data.offlineTime).format("YYYY-MM-DD HH:mm:ss")}】被强制下线！`;

					if (data.message) {
						message += `\r\n原因：${data.message}`;
					}

					await userInfoStore.logout({
						type: 1,
						message,
					});
				}
			);

			/**
			 * 开始连接
			 */
			await connection.start();
		}
	} catch (error) {
		logger.error("WebSocket", error);
	}
};

/**
 * 关闭 WebSocket
 */
const closeWebSocket = async (): Promise<void> => {
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

export { connection as signalR, initWebSocket, closeWebSocket };
