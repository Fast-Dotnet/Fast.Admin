import { ElLoading, ElMessage, ElMessageBox } from "element-plus";
import { createFastAxios } from "@fast-china/axios";
import { AESDecrypt, AESEncrypt, Local, installationIdentity, logger, withDefineType } from "@fast-china/utils";
import { isAxiosError } from "axios";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { useUserInfo } from "@/stores";
import type { ApiResponse } from "@fast-china/axios";
import type { AxiosHeaders, AxiosRequestConfig, AxiosResponse } from "axios";

// 并发请求共享的加载提示状态
const loadingInstance = {
	// ElLoading 的实例信息
	target: withDefineType<ReturnType<typeof ElLoading.service>>(null),
	// 总数
	count: 0,
};

// 防止并发的 401 响应重复弹出登录失效提示。
let loginCallBack = false;

/** 识别业务状态码或 HTTP 401，并请求重新登录。 */
const handleReloadLogin = (response: AxiosResponse) => {
	// 尝试获取 Restful 风格返回Code，或者获取响应状态码
	const code = response?.data?.code || response?.status;
	if (code !== 401) return false;
	if (!loginCallBack) {
		loginCallBack = true;
		ElMessageBox.alert("登录已失效，请重新登录！", {
			title: "温馨提示",
			type: "warning",
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
	let baseUrl = import.meta.env.VITE_API_BASE_URL;
	if (baseUrl?.endsWith("/")) {
		baseUrl = baseUrl.slice(0, -1);
	}

	const fastAxios = createFastAxios({
		baseUrl,
		headers: {
			"Fast-Origin": import.meta.env.DEV ? import.meta.env.VITE_APP_ORIGIN || window.location.host : window.location.host,
			"Fast-Device-Type": Object.entries(AppEnvironmentEnum).find(([, value]) => value === AppEnvironmentEnum.Web)?.[0],
			"Fast-Device-Id": installationIdentity.deviceId,
		},
		requestCipher: true,
	});

	fastAxios.loading.show.use((text) => {
		loadingInstance.count++;
		if (loadingInstance.count === 1) {
			// 合并 Loading 配置
			loadingInstance.target = ElLoading.service({
				fullscreen: true,
				lock: true,
				text: text ?? "加载中...",
				background: "rgba(0, 0, 0, 0.7)",
			});
		} else {
			loadingInstance.target.setText(text ?? "加载中...");
		}
	});
	fastAxios.loading.close.use(() => {
		loadingInstance.count = Math.max(0, loadingInstance.count - 1);
		if (loadingInstance.count === 0) {
			loadingInstance.target.close();
			loadingInstance.target = null;
		}
	});

	fastAxios.message.success.use((message) => message && ElMessage.success(message));
	fastAxios.message.warning.use((message) => message && ElMessage.warning(message));
	fastAxios.message.info.use((message) => message && ElMessage.info(message));
	fastAxios.message.error.use((message) => message && ElMessage.error(message));

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
