import { configureInstallationIdentity, configureLogger, configureStorage, getOrCreateInstallationId, logger } from "@fast-china/utils";
import { isAxiosError } from "axios";
import dayjs from "dayjs";
import "dayjs/locale/zh-cn";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { CommonUniApp } from "@/common";
import { RegExps } from "@/constants";
import { usePrivacy } from "@/hooks/use-privacy";
import { useApp, useConfig } from "@/stores";
import { loadFastAxios } from "./axios";
import { loadWotUi } from "./wot-ui";
import { loadZPaging } from "./z-paging";
import type { App, ComponentPublicInstance } from "vue";

/** 注册存储与 UI 默认配置；设备信息和请求初始化延后至隐私同意后。 */
export function loadPlugins(app: App): void {
	// 全局异常捕获
	app.config.errorHandler = (err, _instance: ComponentPublicInstance, _info: string) => {
		if (isAxiosError(err)) return;
		console.error(err);
		uni.showModal({
			title: "系统错误",
			// eslint-disable-next-line @typescript-eslint/no-base-to-string
			content: err instanceof Error ? err.message : String(err),
			showCancel: false,
		});
	};

	configureLogger({
		level: import.meta.env.DEV ? "debug" : "warn",
	});

	configureStorage({ prefix: "fast__", crypto: import.meta.env.VITE_STORAGE_CRYPTO === "true" });

	dayjs.locale("zh-cn");

	loadWotUi();
	loadZPaging();
	loadAppRuntime();
}

let isRuntimeLoaded = false;

/** 隐私同意后初始化平台信息、请求与主题；重复调用不重复注册监听。 */
export function loadAppRuntime(): void {
	if (isRuntimeLoaded || !usePrivacy.hasAgreed()) return;

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
	// 小程序展示微信发布版本；开发版和体验版可能没有版本名称。
	appStore.appVersion = accountInfo.miniProgram.version || appStore.appVersion || "开发版";
	// #endif

	// #ifdef APP-PLUS || APP-HARMONY
	if (process.env.UNI_PLATFORM === "app-harmony") {
		appStore.deviceType = AppEnvironmentEnum.HarmonyOS;
	} else if (appStore.deviceInfo.platform === "ios") {
		appStore.deviceType = AppEnvironmentEnum.IOS;
	} else {
		appStore.deviceType = AppEnvironmentEnum.Android;
	}

	// #ifdef APP-PLUS
	// WGT 版本名称仅用于 Android/iOS。
	if (appStore.appBaseInfo.appWgtVersion) {
		appStore.appVersion = appStore.appBaseInfo.appWgtVersion;
	}
	// #endif
	// #endif

	// #ifdef H5
	appStore.deviceType = AppEnvironmentEnum.H5;
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

	loadFastAxios();

	// #ifdef APP-PLUS
	// App 需要设置原生 UI 外观，主题才能跟随系统。
	plus.nativeUI.setUIStyle("auto");
	// #endif

	// 设置应用名称
	appStore.setAppName(appStore.appBaseInfo.appName);

	// 初始化主题
	useConfig().initTheme();

	logger.debug("App", `成功加载【${appStore.appBaseInfo.appName}】`, new Date());
	isRuntimeLoaded = true;
}
