import { createFastAxios } from "@fast-china/axios";
import { AESDecrypt, AESEncrypt, Local, installationIdentity, logger, withDefineType } from "@fast-china/utils";
import { isAxiosError } from "axios";
import { useLoading, useMessageBox, useToast } from "@/hooks";
import { useApp, useUserInfo } from "@/stores";
import type { ApiResponse } from "@fast-china/axios";
import type { AxiosHeaders, AxiosRequestConfig, AxiosResponse } from "axios";
import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";

// 并发请求共享的加载提示状态
const loadingInstance = {
	// 加载文案
	texts: withDefineType<string[]>([]),
	// 总数
	count: 0,
};

// 防止并发的 401 响应重复弹出登录失效提示。
let loginCallBack = false;

/** 识别业务状态码或 HTTP 401，并请求重新登录。 */
const handleReloadLogin = (response?: AxiosResponse) => {
	// 尝试获取 Restful 风格返回Code，或者获取响应状态码
	const code = response?.data?.code || response?.status;
	if (code !== 401) return false;
	if (!loginCallBack) {
		loginCallBack = true;
		useMessageBox
			.alert({
				msg: "登录已失效，请重新登录！",
				confirmButtonText: "重新登录",
			})
			.then(() => useUserInfo().logout())
			.finally(() => {
				loginCallBack = false;
			});
	}
	return true;
};

/** 创建请求实例并注册加载提示、缓存、加解密及认证拦截器。 */
export function loadFastAxios(): void {
	const appStore = useApp();

	let baseUrl = `${import.meta.env.VITE_API_REQUEST_URL.replace(/\/$/, "")}${import.meta.env.VITE_API_BASE_URL}`;
	if (baseUrl?.endsWith("/")) {
		baseUrl = baseUrl.slice(0, -1);
	}

	let origin: string;
	let deviceType: keyof typeof AppEnvironmentEnum;
	switch (process.env.UNI_PLATFORM) {
		case "mp-weixin":
			origin = appStore.appId;
			deviceType = "WeChatMiniProgram";
			break;
		case "app":
			origin = appStore.appId;
			deviceType = uni.getSystemInfoSync().platform === "ios" ? "IOS" : "Android";
			break;
		case "h5":
		default:
			origin = import.meta.env.DEV ? import.meta.env.VITE_APP_ORIGIN || window.location.host : window.location.host;
			deviceType = "Web";
			break;
	}

	const fastAxios = createFastAxios({
		baseUrl,
		headers: {
			"Fast-Origin": origin,
			"Fast-Device-Type": deviceType,
			"Fast-Device-Id": installationIdentity.deviceId,
		},
		requestCipher: true,
	});

	fastAxios.loading.show.use((text) => {
		loadingInstance.count++;
		loadingInstance.texts.push(text);
		useLoading.show(text ?? "加载中...");
	});
	fastAxios.loading.close.use(() => {
		loadingInstance.count = Math.max(0, loadingInstance.count - 1);
		loadingInstance.texts.pop();
		if (loadingInstance.count > 0) {
			// 获取下一个加载文案
			useLoading.show(loadingInstance.texts[loadingInstance.texts.length - 1]);
		} else {
			useLoading.hide();
		}
	});

	fastAxios.message.success.use((message) => message && useToast.success(message));
	fastAxios.message.warning.use((message) => message && useToast.warning(message));
	fastAxios.message.info.use((message) => message && useToast.info(message));
	fastAxios.message.error.use((message) => message && useToast.error(message));

	fastAxios.cache.get.use((key) => Local.get(`HTTP_CACHE_${key}`));
	fastAxios.cache.set.use((key, value) => Local.set(`HTTP_CACHE_${key}`, value, { ttlMs: 24 * 60 * 60 * 1000 }));

	fastAxios.crypto.encrypt.use((config: AxiosRequestConfig, timestamp) => {
		const requestData = config.data ?? config.params;
		const dataStr = JSON.stringify(requestData);
		if (dataStr !== undefined && dataStr !== "" && dataStr !== "{}") {
			logger.debug("Fast-Axios", `HTTP request data("${config.url}")`, requestData);
			// 组装请求格式
			const encryptedRequestData = {
				data: AESEncrypt(dataStr, `${timestamp}`, `FIV${timestamp}`),
				timestamp,
			};
			if (["GET", "DELETE", "HEAD"].includes(config.method?.toUpperCase())) {
				config.params = encryptedRequestData;
			} else {
				config.data = encryptedRequestData;
			}
			// 请求头部增加加密标识
			config.headers["Fast-Request-Encipher"] = "true";
		}
	});

	fastAxios.crypto.decrypt.use((response) => {
		const restfulData = response.data as ApiResponse;
		const responseHeader = response.headers as AxiosHeaders;
		// 判断响应头部是否有加密标识
		if (responseHeader.get("Fast-Response-Encipher")?.toString()?.toLowerCase() === "true" && restfulData?.data) {
			restfulData.data = AESDecrypt(restfulData.data as string, `${restfulData.timestamp}`, `FIV${restfulData.timestamp}`).parseJson();
			// 处理 ""xxx"" 这种数据
			if (typeof restfulData.data === "string" && restfulData.data.startsWith('"') && restfulData.data.endsWith('"')) {
				restfulData.data = restfulData.data.replace(/"/g, "");
			}
			logger.debug("Fast-Axios", `HTTP response data("${response.config.url}")`, restfulData.data);
		}
		return restfulData;
	});

	fastAxios.interceptors.request.use((config) => {
		const { token, refreshToken } = useUserInfo().resolveToken();
		if (token) config.headers["Authorization"] = token;
		// 刷新 Token
		if (refreshToken) config.headers["X-Authorization"] = refreshToken;
	});

	fastAxios.interceptors.response.use((response) => {
		useUserInfo().setToken(response);
		return handleReloadLogin(response) ? (response?.data ?? response) : undefined;
	});

	fastAxios.interceptors.responseError.use((error) => {
		if (!isAxiosError(error) || !error.response) return undefined;
		useUserInfo().setToken(error.response);
		return handleReloadLogin(error.response) ? (error.response.data ?? error.response) : undefined;
	});
}
