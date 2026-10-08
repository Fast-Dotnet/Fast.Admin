import { defineStore } from "pinia";
import { reactive, ref, toRefs } from "vue";
import { Local, decodeBase64Url, logger } from "@fast-china/utils";
import { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { authApi } from "@/api/services/Auth/auth";
import { loginApi } from "@/api/services/Auth/login";
import { CommonRoute } from "@/common";
import { useMessageBox, useToast } from "@/hooks";
import { closeWebSocket } from "@/signalR";
import { useApp } from "../app";
import type { AxiosResponse } from "axios";
import type { GetLoginUserInfoOutput } from "@/api/services/Auth/auth/models/GetLoginUserInfoOutput";
import type { LoginOutput } from "@/api/services/Auth/login/models/LoginOutput";

type IState = {
	/** Token */
	token: string;
	/** Refresh Token */
	refreshToken: string;
	/** 激活的 TabBar 页面 */
	activeTabBar: string;
	/** 授权登录弹窗 */
	authLoginPopup: boolean;
};

/** 当前账号、租户与登录凭据状态 */
export const useUserInfo = defineStore(
	"userInfo",
	() => {
		const state = reactive<IState & Required<GetLoginUserInfoOutput>>({
			token: "",
			refreshToken: "",
			activeTabBar: CommonRoute.Workbench,
			authLoginPopup: false,
			accountId: undefined,
			accountKey: "",
			mobile: "",
			nickName: "",
			avatar: "",
			identityVerification: false,
			tenantNo: "",
			tenantName: "",
			shortName: "",
			tenantCode: "",
			logoUrl: "",
			userKey: "",
			employeeId: undefined,
			employeeNo: "",
			employeeName: "",
			departmentId: null,
			departmentName: "",
			isSuperAdmin: false,
			isAdmin: false,
			roleNameList: [],
			buttonCodeList: [],
			roleType: null,
			dataScopeType: DataScopeTypeEnum.All,
			menuList: [],
		});

		// 是否存在用户信息
		const hasUserInfo = ref(false);

		// WebSocket 是否连接
		const hasWebSocket = ref(false);

		/** 合并服务端返回的用户信息。 */
		const setUserInfo = (uInfo: GetLoginUserInfoOutput) => {
			Object.assign(state, uInfo);
		};

		/** 删除 Token。 */
		const removeToken = () => {
			state.token = "";
			state.refreshToken = "";
			// 删除Token后，不用校验账号
			state.identityVerification = false;
		};

		/** 读取响应头中的双 Token；服务端标记访问 Token 失效时清除本地凭据。 */
		const setToken = (axiosResponse: AxiosResponse) => {
			if (typeof axiosResponse?.headers?.get !== "function") return;
			// 从响应头获取访问 Token
			const token = axiosResponse.headers.get("access-token")?.toString();
			// 从响应头获取刷新 Token
			const refreshToken = axiosResponse.headers.get("x-access-token")?.toString();
			// 判断是否为无效 Token
			if (token === "invalid_token") {
				// 删除 Token
				removeToken();
			} else if (token && refreshToken && refreshToken !== "invalid_token") {
				// 设置 Token
				state.token = token;
				state.refreshToken = refreshToken;
			}
		};

		/**
		 * 获取当前 Pinia 状态中的 Token。
		 */
		const getToken = () => {
			return { token: state.token, refreshToken: state.refreshToken };
		};

		/**
		 * 解析 Token 载荷；访问 Token 过期时同时返回刷新 Token。
		 *
		 * @remarks
		 * 此处仅解码载荷，不校验签名，也不发起刷新请求；格式错误会抛出异常。
		 *
		 * @param token - 访问 Token；null 或 undefined 时使用 Pinia 中的值
		 * @param refreshToken - 刷新 Token；null 或 undefined 时使用 Pinia 中的值
		 */
		const resolveToken = (
			token: string = null,
			refreshToken: string = null
		): {
			token: string;
			refreshToken: string;
			tokenData: unknown;
		} => {
			token ??= state.token;
			refreshToken ??= state.refreshToken;
			if (token) {
				const jwtToken = decodeURIComponent(encodeURIComponent(decodeBase64Url(token.replace(/_/g, "/").replace(/-/g, "+").split(".")[1])));
				const jwtTokenData = JSON.parse(jwtToken) as { exp?: number };
				// 获取 Token 的过期时间
				const expired = Date.now() >= jwtTokenData.exp * 1000;
				if (expired) {
					return { token: `Bearer ${token}`, refreshToken: `Bearer ${refreshToken}`, tokenData: jwtTokenData };
				}
				return { token: `Bearer ${token}`, refreshToken: null, tokenData: jwtTokenData };
			}
			return { token: null, refreshToken: null, tokenData: null };
		};

		/** 登录。 */
		const login = (loginRes: LoginOutput) => {
			if (!loginRes || loginRes.status !== LoginStatusEnum.Success) return;
			// 优先缓存一些数据
			state.accountKey = loginRes.accountKey;
			state.nickName = loginRes.nickName;
			state.avatar = loginRes.avatar;
			const userRes = loginRes.tenantList[0];
			state.userKey = userRes.userKey;
			state.tenantName = userRes.tenantName;
			state.employeeNo = userRes.employeeNo;
			state.employeeName = userRes.employeeName;
			useToast.success("登录成功");
			// 确保 getLoginUser 获取用户信息
			hasUserInfo.value = false;
			// 跳转到工作台
			uni.switchTab({ url: CommonRoute.Workbench });
		};

		/** 清除登录凭据与 HTTP 缓存，并导航到登录页。 */
		const logoutClear = () => {
			removeToken();
			// 删除 HTTP 缓存数据
			Local.removeByPrefix("HTTP_CACHE_");

			const currentPage = getCurrentPages().at(-1) as { route?: string; $page?: { fullPath?: string } };
			const currentPath = currentPage?.route ? `/${currentPage.route.replace(/^\/+/, "")}` : "";
			// 排除登录页面报错的问题
			if (currentPath === CommonRoute.Login) {
				uni.navigateTo({ url: CommonRoute.Login });
			} else {
				const fullPath = currentPage?.$page?.fullPath || currentPath;
				const redirect = fullPath ? `?redirect=${encodeURIComponent(fullPath)}` : "";
				uni.reLaunch({ url: `${CommonRoute.Login}${redirect}` });
			}
		};

		/** 退出登录。 */
		const logout = async (
			data: {
				/**
				 * 1 强制下线
				 * 2 其他地方登录
				 */
				type: 1 | 2;
				message: string;
			} = null
		) => {
			if (data?.type === 2) {
				useMessageBox.alert(data?.message);
				try {
					// 关闭 WebSocket 连接
					closeWebSocket();
				} catch (error) {
					logger.error("Logout", error);
				}
				logoutClear();
			} else {
				try {
					// 关闭 WebSocket 连接
					closeWebSocket();
				} catch (error) {
					logger.error("Logout", error);
				}
				try {
					await loginApi.logout();
				} catch (error) {
					logger.error("Logout", error);
				} finally {
					logoutClear();
					if (data !== null) {
						useMessageBox.alert(data?.message);
					}
				}
			}
		};

		/** 请求并合并当前用户信息；请求失败继续向调用方抛出异常。 */
		const refreshUserInfo = async () => {
			const apiRes = await authApi.getLoginUserInfo();
			setUserInfo(apiRes);
		};

		/** 刷新应用。 */
		const refreshApp = async () => {
			// 删除 HTTP 缓存数据
			Local.removeByPrefix("HTTP_CACHE_");

			// 刷新用户信息
			await refreshUserInfo();

			// 刷新字典
			await useApp().setDictionary();
		};

		/** 清除登录状态并返回登录页，使下次导航重新加载用户信息。 */
		const switchLogin = () => {
			// 确保下次会自动刷新用户信息
			hasUserInfo.value = false;
			logoutClear();
		};

		/** 检查手机号是否已存在；缺失时打开授权弹窗并返回 false。 */
		const authLoginCheck = (): boolean => {
			if (!state.mobile) {
				state.authLoginPopup = true;
				return false;
			}
			return true;
		};

		/** 获取微信小程序登录 code；授权失败或非微信小程序平台时返回空字符串。 */
		const getWeChatCode = () =>
			new Promise<string>((resolve) => {
				// #ifdef MP-WEIXIN
				uni.login({
					success: ({ code }) => resolve(code),
					fail: () => {
						useToast.warning("授权失败，无法获取您的信息。请重新授权以继续使用我们的服务。");
						resolve("");
					},
				});
				// #endif

				// #ifndef MP-WEIXIN
				resolve("");
				// #endif
			});

		return {
			...toRefs(state),
			hasUserInfo,
			hasWebSocket,
			setUserInfo,
			removeToken,
			setToken,
			getToken,
			resolveToken,
			login,
			logout,
			logoutClear,
			refreshUserInfo,
			refreshApp,
			switchLogin,
			authLoginCheck,
			getWeChatCode,
		};
	},
	{
		persist: {
			key: "store-user-info",
			// 仅持久化登录凭据和选定的账号展示字段；用户信息加载状态不持久化。
			pick: [
				"token",
				"refreshToken",
				"accountKey",
				"nickName",
				"avatar",
				"tenantName",
				"userKey",
				"employeeNo",
				"employeeName",
				"departmentName",
				"isSuperAdmin",
				"isAdmin",
			],
		},
	}
);
