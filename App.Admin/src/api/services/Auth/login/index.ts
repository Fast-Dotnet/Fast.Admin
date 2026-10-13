import { axiosUtil } from "@fast-china/axios";
import type { LoginCaptchaOutput } from "./models/LoginCaptchaOutput";
import type { LoginInput } from "./models/LoginInput";
import type { LoginOutput } from "./models/LoginOutput";
import type { LoginTenantOutput } from "./models/LoginTenantOutput";
import type { TenantLoginInput } from "./models/TenantLoginInput";
import type { TryLoginInput } from "./models/TryLoginInput";
import type { WeChatAuthLoginInput } from "./models/WeChatAuthLoginInput";
import type { WeChatClientLoginInput } from "./models/WeChatClientLoginInput";
import type { WeChatClientLoginOutput } from "./models/WeChatClientLoginOutput";
import type { WeChatLoginInput } from "./models/WeChatLoginInput";

/**
 * 登录服务Api
 */
export const loginApi = {
	/**
	 * 登录
	 */
	login(data: LoginInput): Promise<LoginOutput> {
		return axiosUtil.request<LoginOutput>({
			url: "/login",
			method: "post",
			data,
			requestType: "auth",
			loading: true,
			loadingText: "登录中...",
		});
	},
	/**
	 * 获取登录用户
	 */
	queryLoginUser(): Promise<LoginTenantOutput[]> {
		return axiosUtil.request<LoginTenantOutput[]>({
			url: "/queryLoginUser",
			method: "get",
			requestType: "query",
		});
	},
	/**
	 * 租户登录
	 */
	tenantLogin(data: TenantLoginInput): Promise<LoginOutput> {
		return axiosUtil.request<LoginOutput>({
			url: "/tenantLogin",
			method: "post",
			data,
			requestType: "auth",
			loading: true,
			loadingText: "租户登录中...",
		});
	},
	/**
	 * 获取登录图片验证码
	 */
	getLoginCaptcha(isForce: boolean): Promise<LoginCaptchaOutput> {
		return axiosUtil.request<LoginCaptchaOutput>({
			url: "/getLoginCaptcha",
			method: "post",
			params: {
				isForce,
			},
			cancelDuplicateRequest: false,
			requestType: "auth",
		});
	},
	/**
	 * 尝试登录
	 */
	tryLogin(data: TryLoginInput): Promise<LoginOutput> {
		return axiosUtil.request<LoginOutput>({
			url: "/tryLogin",
			method: "post",
			data,
			requestType: "auth",
			showCodeMessage: false,
		});
	},
	/**
	 * 退出登录
	 */
	logout(): Promise<void> {
		return axiosUtil.request<void>({
			url: "/logout",
			method: "post",
			requestType: "auth",
			loading: true,
		});
	},
	/**
	 * 微信登录
	 */
	weChatLogin(data: WeChatLoginInput): Promise<LoginOutput> {
		return axiosUtil.request<LoginOutput>({
			url: "/weChatLogin",
			method: "post",
			data,
			requestType: "auth",
			loading: true,
			loadingText: "登录中...",
		});
	},
	/**
	 * 微信授权登录
	 */
	weChatAuthLogin(data: WeChatAuthLoginInput): Promise<LoginOutput> {
		return axiosUtil.request<LoginOutput>({
			url: "/weChatAuthLogin",
			method: "post",
			data,
			requestType: "auth",
			loading: true,
			loadingText: "登录中...",
		});
	},
	/**
	 * 微信客户端登录
	 */
	weChatClientLogin(data: WeChatClientLoginInput): Promise<WeChatClientLoginOutput> {
		return axiosUtil.request<WeChatClientLoginOutput>({
			url: "/weChatClientLogin",
			method: "post",
			data,
			requestType: "auth",
			loading: true,
			loadingText: "登录中...",
		});
	},
};
