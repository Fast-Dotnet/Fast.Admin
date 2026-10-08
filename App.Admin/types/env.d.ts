/// <reference types="vite/client" />

/** 声明 vite 环境变量的类型（如果未声明则默认是 any） */
declare interface ImportMetaEnv {
	/**
	 * 接口基础地址
	 */
	readonly VITE_API_BASE_URL: string;
	/**
	 * 接口请求地址
	 */
	readonly VITE_API_REQUEST_URL: string;
	/**
	 * 本地缓存加密
	 */
	readonly VITE_STORAGE_CRYPTO: "true" | "false";
	/**
	 * APP 来源
	 */
	readonly VITE_APP_ORIGIN: string;
}
