import { defineStore } from "pinia";
import { reactive, shallowRef, toRefs } from "vue";
import { useFastAxios } from "@fast-china/axios";
import { Local, getOrCreateInstallationId, installationIdentity, logger } from "@fast-china/utils";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import { appApi } from "@/api/services/Center/app";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import type { LaunchOutput } from "@/api/services/Center/app/models/LaunchOutput";

export const defaultThemeColor = "#409EFF";

type IState = {
	/** 是否存在 Launch 数据 */
	hasLaunch: boolean;
	/** ICP 备案号 */
	icpSecurityCode: string;
	/** 公安备案号 */
	publicSecurityCode: string;
	/** 环境 */
	env: ViteEnv;
	/** 设备类型 */
	deviceType: AppEnvironmentEnum;
	/** 当前应用的AppId，获取到的 */
	appId: string;
	/**
	 * - manifest.json 中应用版本名称
	 * - 如果存在热更新则为 应用资源（wgt）的版本名称
	 */
	appVersion: string;
	/** 是否为 IPhone 设备 */
	isIphone: boolean;
	/** 是否为客户端（Pc） */
	isClient: boolean;
	/** 设备基础信息 */
	appBaseInfo: UniNamespace.GetAppBaseInfoResult;
	/** 设备信息 */
	deviceInfo: UniNamespace.GetDeviceInfoResult;
	/** 窗口信息 */
	windowInfo: UniNamespace.GetWindowInfoResult;
	/**
	 * 小程序胶囊按钮信息
	 * @description 非小程序平台下，只有默认高度
	 */
	menuButton: UniNamespace.GetMenuButtonBoundingClientRectRes;
	/** 网络 */
	network: {
		/** 是否在线 */
		onLine: boolean;
		/** 网络类型 */
		networkType: INetworkType;
	};
};

export const useApp = defineStore(
	"app",
	() => {
		const state = reactive<IState & Required<LaunchOutput>>({
			edition: EditionEnum.None,
			appNo: "",
			appName: "Fast.App",
			logoUrl: "",
			themeColor: defaultThemeColor,
			appType: AppEnvironmentEnum.WeChatMiniProgram,
			environmentType: EnvironmentTypeEnum.Development,
			loginComponent: "ClassicLogin",
			webSocketUrl: "",
			requestTimeout: 6000,
			requestEncipher: true,
			tenantName: "",
			hasLaunch: false,
			icpSecurityCode: "",
			publicSecurityCode: "",
			env: "production",
			deviceType: AppEnvironmentEnum.WeChatMiniProgram,
			appId: "",
			appVersion: "",
			isIphone: false,
			isClient: false,
			appBaseInfo: null,
			deviceInfo: null,
			windowInfo: null,
			menuButton: null,
			network: { onLine: false, networkType: "unknown" },
		});

		/** 字典 */
		const dictionary = shallowRef<Map<string, FaTableEnumColumnCtx[]>>(new Map());

		/** 设置App名称 */
		const setAppName = (appName: string) => {
			if (!appName) return;
			state.appName = appName;
		};

		/** 设置字典 */
		const setDictionary = async () => {
			// 判断是否存在 Launch 数据
			if (!state.hasLaunch) return;
			try {
				dictionary.value.clear();
				// 处理数据字典
				const _dictionary = await dictionaryApi.queryDictionary();
				Object.entries(_dictionary).forEach(([key, value]) => {
					dictionary.value.set(key, value);
				});
			} catch {
				logger.error("App", "字典加载失败");
			}
		};

		/** 设置 FastAxios */
		const setFastAxios = () => {
			// 判断是否存在 Launch 数据
			if (!state.hasLaunch) return;
			useFastAxios().setOptions({
				timeout: state.requestTimeout,
				requestCipher: state.requestEncipher,
			});
		};

		/** Launch */
		const launch = async () => {
			try {
				const apiRes = await appApi.launch();
				logger.log("Launch", apiRes);
				Object.assign(state, apiRes);
				state.hasLaunch = true;
			} finally {
				state.loginComponent ||= "ClassicLogin";

				uni.setNavigationBarTitle({ title: state.appName });

				setFastAxios();

				// 处理数据字典
				await setDictionary();
			}
		};

		/** 获取字典 */
		const getDictionary = (key: string, includeAll = false): FaTableEnumColumnCtx[] => {
			if (!dictionary.value.has(key)) {
				logger.error("app", `字典 [${key}] 不存在`);
				return null;
			}
			const items = dictionary.value.get(key);
			return includeAll ? [{ label: "全部", value: null }, ...items] : items;
		};

		/** 清除 App 缓存 */
		const clearAppCache = () => {
			// 获取设备Id
			const deviceId = installationIdentity.deviceId;
			// 清空 Local 缓存
			Local.clear();
			// 重新设置设备Id
			getOrCreateInstallationId(deviceId);
		};

		return {
			...toRefs(state),
			setAppName,
			setDictionary,
			setFastAxios,
			launch,
			getDictionary,
			clearAppCache,
		};
	},
	{
		persist: {
			key: "store-app",
		},
	}
);
