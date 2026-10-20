import { defineStore } from "pinia";
import { reactive, ref, toRefs } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { Local, logger } from "@fast-china/utils";
import { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import { authApi } from "@/api/services/Auth/auth";
import { loginApi } from "@/api/services/Auth/login";
import router from "@/router";
import { closeWebSocket } from "@/signalR";
import type { AxiosResponse } from "axios";
import type { GetLoginUserInfoOutput } from "@/api/services/Auth/auth/models/GetLoginUserInfoOutput";

type IState = {
	/** Token */
	token: string;
	/** Refresh Token */
	refreshToken: string;
};

/** 当前账号、租户与登录凭据状态 */
export const useUserInfo = defineStore(
	"userInfo",
	() => {
		const state = reactive<IState & Required<GetLoginUserInfoOutput>>({
			token: "",
			refreshToken: "",
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

		// 动态生成路由
		const asyncRouterGen = ref(false);

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
				const jwtToken = decodeURIComponent(encodeURIComponent(window.atob(token.replace(/_/g, "/").replace(/-/g, "+").split(".")[1])));
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
		const login = () => {
			ElMessage.success("登录成功");
			// 确保 getLoginUser 获取用户信息
			asyncRouterGen.value = false;
			// 进入系统
			router.push("/");
		};

		/** 清除登录凭据与 HTTP 缓存，并导航到登录页。 */
		const logoutClear = () => {
			removeToken();
			// 删除 HTTP 缓存数据
			Local.removeByPrefix("HTTP_CACHE_");
			// 排除登录页面报错的问题
			if (router.currentRoute.value.path === "/login") {
				router.push({ path: "/login" });
			} else {
				router.push({ path: "/login", query: { redirect: router.currentRoute.value.fullPath } });
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
				ElMessageBox.alert(data?.message, { type: "warning" });
				try {
					// 关闭 WebSocket 连接
					await closeWebSocket();
				} catch (error) {
					logger.error("Logout", error);
				}
				logoutClear();
			} else {
				try {
					// 关闭 WebSocket 连接
					await closeWebSocket();
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
						ElMessageBox.alert(data?.message, {
							type: "warning",
						});
					}
				}
			}
		};

		/** 请求并合并当前用户信息；请求失败继续向调用方抛出异常。 */
		const refreshUserInfo = async () => {
			const apiRes = await authApi.getLoginUserInfo();
			setUserInfo(apiRes);
		};

		return {
			...toRefs(state),
			asyncRouterGen,
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
		};
	},
	{
		persist: {
			key: "store-user-info",
			// 这里是配置 pinia 只需要持久化 state 中的 Token 即可，而不是整个 store
			pick: ["token", "refreshToken"],
		},
	}
);
