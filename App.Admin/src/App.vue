<script setup lang="ts">
import { onLaunch } from "@dcloudio/uni-app";
import { watch } from "vue";
import { axiosUtil } from "@fast-china/axios";
import { configureInstallationIdentity, getOrCreateInstallationId, logger } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { CommonRoute, CommonUniApp } from "@/common";
import { RegExps } from "@/constants";
import { useUpdate } from "@/hooks";
import { usePrivacy } from "@/hooks/use-privacy";
import { loadFastAxios } from "@/plugins/axios";
import { useApp, useConfig } from "@/stores";
import type { LaunchOutput } from "@/api/services/Center/app/models/LaunchOutput";

const appStore = useApp();
const configStore = useConfig();
const router = useRouter();

onLaunch((options) => {
	// #ifdef MP
	// 冷启动时尽早监听更新，后台下载完成后会自动提示重启。
	useUpdate.checkUpdate();
	// #endif

	loadFastAxios();

	axiosUtil
		.request<LaunchOutput>({
			url: "/launch",
			method: "post",
			requestType: "auth",
			timeout: 10_000,
			showErrorMessage: false,
			showCodeMessage: false,
		})
		.then((apiRes) => {
			Object.assign(appStore, apiRes);
			appStore.hasLaunch = true;

			appStore.setFastAxios();
		})
		.catch((error: unknown) => {
			logger.warn("App", "启动配置加载失败", error);
		})
		.finally(async () => {
			if (!appStore.hasLaunch) {
				// #ifdef H5
				uni.showModal({
					title: "服务连接失败",
					content: "暂时无法加载应用配置，请检查网络后重新加载。",
					showCancel: false,
					confirmText: "重新加载",
					success: () => window.location.reload(),
				});
				// #endif

				// #ifndef H5
				if (router.currentRoute.value.path !== CommonRoute.Launcher) {
					await router.replaceAll(CommonRoute.Launcher);
				}
				uni.showModal({
					title: "服务连接失败",
					content: "暂时无法加载应用配置，请检查网络后稍后重新打开应用。",
					showCancel: false,
					confirmText: "确定",
				});
				// #endif
				return;
			}
		});

	watch(
		() => usePrivacy.hasAgreed(),
		(newValue) => {
			if (!newValue) return;

			configureInstallationIdentity();
			const deviceId = getOrCreateInstallationId();
			logger.debug("App", "Env", import.meta.env);
			logger.debug("App", "DeviceId", deviceId);

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

			// #ifdef APP-PLUS
			// App 需要设置原生 UI 外观，主题才能跟随系统。
			plus.nativeUI.setUIStyle("auto");
			// #endif

			// 未取得启动配置时使用本地应用名称，保留先于隐私同意返回的服务端名称。
			if (!appStore.hasLaunch) appStore.setAppName(appStore.appBaseInfo.appName);

			// 初始化主题
			configStore.initTheme();

			logger.debug("App", `成功加载【${appStore.appBaseInfo.appName}】`, new Date());

			// 加载字典
			appStore.setDictionary();
		},
		// 同步完成初始化，启动页随后才能检查更新和登录。
		{ immediate: true, flush: "sync" }
	);

	// #ifdef H5
	// 首次进入时重新导航当前页面，触发路由守卫并保留页面参数。
	const launchPath = `/${options.path}`;

	// 启动页自行处理登录跳转，其他入口重新导航以触发首屏路由守卫。
	if (launchPath !== "/" && launchPath !== CommonRoute.Launcher) {
		router.replaceAll({ path: launchPath, query: options.query });
	}
	// #endif
});
</script>
