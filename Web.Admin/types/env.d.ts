/** 声明 vite 环境变量的类型（如果未声明则默认是 any） */
declare interface ImportMetaEnv {
	/**
	 * 运行端口号
	 */
	readonly VITE_PORT: string;
	/**
	 * 网站根目录
	 */
	readonly VITE_PUBLIC_PATH: string;
	/**
	 * 静态资源公共地址，可配置为 CDN 地址
	 */
	readonly STATIC_ASSET_BASE_URL: string;
	/**
	 * CDN 地址
	 */
	readonly CDN_URL: string;
	/**
	 * 构建输出目录
	 */
	readonly BUILD_OUT_DIR: string;
	/**
	 * 接口基础地址
	 */
	readonly VITE_API_BASE_URL: string;
	/**
	 * 本地缓存加密
	 */
	readonly VITE_STORAGE_CRYPTO: "true" | "false";
	/**
	 * 启用移动端访问
	 */
	readonly VITE_ENABLE_MOBILE: "true" | "false";
	/**
	 * 启用 Web 与 H5 自动跳转
	 */
	readonly VITE_ENABLE_BROWSER_REDIRECT: "true" | "false";
	/**
	 * 应用版本号
	 */
	readonly VITE_APP_VERSION: string;
	/**
	 * APP 来源
	 */
	readonly VITE_APP_ORIGIN: string;
	/**
	 * 接口代理地址
	 */
	readonly API_PROXY_URL: string;
}
