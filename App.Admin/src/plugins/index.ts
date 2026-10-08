import { type App, type ComponentPublicInstance, nextTick } from "vue";
import { configureInstallationIdentity, configureLogger, configureStorage, getOrCreateInstallationId, logger } from "@fast-china/utils";
import dayjs from "dayjs";
import "dayjs/locale/zh-cn";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { CommonUniApp } from "@/common";
import { RegExps } from "@/constants";
import { useMessageBox } from "@/hooks";
import { useApp } from "@/stores";
import { loadFastAxios } from "./axios";
import { loadWotUi } from "./wot-ui";
import { loadZPaging } from "./z-paging";

/** 初始化平台信息、存储、请求和 UI 默认配置；调用前须先安装 Pinia。 */
export function loadPlugins(app: App): void {
	// 全局异常捕获
	app.config.errorHandler = (err, _instance: ComponentPublicInstance, _info: string) => {
		if (!err) return;
		const errorMap: Record<string, string> = {
			InternalError: "Javascript引擎内部错误",
			ReferenceError: "未找到对象",
			TypeError: "使用了错误的类型或对象",
			RangeError: "使用内置对象时，参数超范围",
			SyntaxError: "语法错误",
			EvalError: "错误的使用了Eval",
			URIError: "URI错误",
			AggregateError: "未知的多个错误",
			TimeoutError: "操作超时",
			NetworkError: "网络错误",
			OutOfMemoryError: "内存溢出",
			DOMException: "DOM 操作异常",
			SecurityError: "安全错误，可能涉及跨域或 CSP 限制",
			EventError: "事件处理错误",
		};
		const errorName = err instanceof Error ? err.name : undefined;
		if (err === "cancel") {
			console.warn("操作已取消");
		} else if (errorName === "AxiosError") {
			return;
		} else {
			const errorMessage = (errorName && errorMap[errorName]) || "未知错误";
			console.error(err);
			nextTick(() => {
				useMessageBox.alert({
					title: "系统错误",
					msg: errorMessage,
				});
			});
		}
	};

	configureLogger({
		level: import.meta.env.DEV ? "debug" : "warn",
	});

	configureStorage({ prefix: "fast__", crypto: import.meta.env.VITE_STORAGE_CRYPTO === "true" });

	configureInstallationIdentity();
	const deviceId = getOrCreateInstallationId();
	logger.debug("App", "Env", import.meta.env);
	logger.debug("App", "DeviceId", deviceId);

	const appStore = useApp();

	// 获取网络信息
	uni.getNetworkType({
		success: ({ networkType }) => {
			const _networkType = networkType as INetworkType;
			logger.debug("App", "NetworkType", _networkType);
			appStore.network = {
				onLine: _networkType !== "none",
				networkType: _networkType,
			};
		},
	});

	// 监听网络变化
	uni.onNetworkStatusChange((res) => {
		logger.debug("App", "监听到网络改变", res);
		appStore.network = {
			onLine: res.isConnected,
			networkType: res.networkType as INetworkType,
		};
	});

	// #ifdef MP-WEIXIN || H5 || APP-PLUS
	// 获取系统信息
	const appBaseInfo = uni.getAppBaseInfo();
	appStore.appBaseInfo = appBaseInfo;

	// 获取设备信息
	const deviceInfo = uni.getDeviceInfo();
	appStore.deviceInfo = deviceInfo;

	// 获取窗口信息
	appStore.windowInfo = uni.getWindowInfo();
	// #endif

	// #ifndef MP-WEIXIN || H5 || APP-PLUS
	const systemInfo = uni.getSystemInfoSync();
	appStore.appBaseInfo = {
		appId: systemInfo.appId,
		appName: systemInfo.appName,
		appVersion: systemInfo.appVersion,
		appVersionCode: systemInfo.appVersionCode,
		appWgtVersion: systemInfo.appWgtVersion,
		language: systemInfo.language,
		version: systemInfo.version,
		hostName: systemInfo.hostName,
		hostVersion: systemInfo.hostVersion,
		hostLanguage: systemInfo.hostLanguage,
		hostTheme: systemInfo.hostTheme,
		hostPackageName: systemInfo.hostPackageName,
		theme: systemInfo.theme,
		SDKVersion: systemInfo.SDKVersion,
		enableDebug: systemInfo.enableDebug,
		host: systemInfo.host,
		appLanguage: systemInfo.appLanguage,
		hostFontSizeSetting: systemInfo.hostFontSizeSetting,
		hostSDKVersion: systemInfo.hostSDKVersion,
	};
	appStore.deviceInfo = {
		deviceBrand: systemInfo.deviceBrand,
		deviceModel: systemInfo.deviceModel,
		deviceId: systemInfo.deviceId,
		deviceType: systemInfo.deviceType,
		devicePixelRatio: systemInfo.devicePixelRatio,
		deviceOrientation: systemInfo.deviceOrientation,
		brand: systemInfo.brand,
		model: systemInfo.model,
		system: systemInfo.system,
		platform: systemInfo.platform,
	};
	appStore.windowInfo = {
		pixelRatio: systemInfo.pixelRatio,
		screenWidth: systemInfo.screenWidth,
		screenHeight: systemInfo.screenHeight,
		windowWidth: systemInfo.windowWidth,
		windowHeight: systemInfo.windowHeight,
		statusBarHeight: systemInfo.statusBarHeight,
		windowTop: systemInfo.windowTop,
		windowBottom: systemInfo.windowBottom,
		safeArea: systemInfo.safeArea,
		safeAreaInsets: systemInfo.safeAreaInsets,
		screenTop: 0,
	};
	// #endif

	// AppId
	appStore.appId = appStore.appBaseInfo.appId;
	// App版本号
	appStore.appVersion = appStore.appBaseInfo.appVersion;
	appStore.menuButton = {
		width: 0,
		height: CommonUniApp.navbarCapsuleMenuButtonHeight,
		top: 0,
		right: 0,
		bottom: 0,
		left: 0,
	};

	// #ifdef MP-WEIXIN
	appStore.deviceType = AppEnvironmentEnum.WeChatMiniProgram;

	// 获取小程序菜单胶囊按钮
	appStore.menuButton = uni.getMenuButtonBoundingClientRect();

	// 获取微信小程序信息
	const accountInfo = uni.getAccountInfoSync();
	// 使用微信小程序的 AppId
	appStore.appId = accountInfo.miniProgram.appId;
	// #endif

	// #ifdef APP-PLUS
	if (appStore.deviceInfo.platform === "ios") {
		appStore.deviceType = AppEnvironmentEnum.IOS;
	} else {
		appStore.deviceType = AppEnvironmentEnum.Android;
	}

	// 判断是否存在 WGT 热更新的版本号
	if (appStore.appBaseInfo.appWgtVersion) {
		appStore.appVersion = appStore.appBaseInfo.appWgtVersion;
	}
	// #endif

	// 是否为 Iphone 设备
	const isIphone = RegExps.IPhone.test(appStore.deviceInfo.model);
	appStore.isIphone = isIphone;

	const isClient =
		appStore.deviceInfo.platform === "windows" || appStore.deviceInfo.platform === "mac" || appStore.deviceInfo.platform === "devtools";
	appStore.isClient = isClient;

	logger.debug("App", "AppBaseInfo", appStore.appBaseInfo);
	logger.debug("App", "DeviceInfo", appStore.deviceInfo);
	logger.debug("App", "WindowInfo", appStore.windowInfo);
	logger.debug("App", "AppId", appStore.appId);
	logger.debug("App", "AppVersion", appStore.appVersion);
	logger.debug("App", "MenuButton", appStore.menuButton);
	logger.debug("App", "IsIphone", isIphone);
	logger.debug("App", "IsClient", isClient);

	dayjs.locale("zh-cn");

	loadFastAxios();

	loadWotUi();

	loadZPaging();
}
