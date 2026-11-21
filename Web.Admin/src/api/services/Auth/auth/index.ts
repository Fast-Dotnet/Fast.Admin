import { axiosUtil } from "@fast-china/axios";
import type { GetLoginUserInfoOutput } from "./models/GetLoginUserInfoOutput";

/**
 * 鉴权服务Api
 */
export const authApi = {
	/**
	 * 刷新Token
	 */
	refreshToken(): Promise<unknown> {
		return axiosUtil.request({
			url: "/refreshToken",
			method: "get",
			requestType: "auth",
		});
	},
	/**
	 * 获取登录用户信息
	 */
	getLoginUserInfo(): Promise<GetLoginUserInfoOutput> {
		return axiosUtil.request<GetLoginUserInfoOutput>({
			url: "/getLoginUserInfo",
			method: "get",
			requestType: "auth",
		});
	},
};
