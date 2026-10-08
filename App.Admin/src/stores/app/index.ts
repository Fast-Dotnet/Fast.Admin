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

/** 应用默认主题色 */
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
	/** 运行平台提供的应用 ID */
	appId: string;
	/**
	 * - manifest.json 中应用版本名称
	 * - 如果存在热更新则为 应用资源（wgt）的版本名称
	 */
	appVersion: string;
	/** 是否为 iPhone 设备 */
	isIphone: boolean;
	/** 是否为桌面设备 */
	isClient: boolean;
	/** 设备基础信息 */
	appBaseInfo: UniNamespace.GetAppBaseInfoResult;
	/** 设备信息 */
	deviceInfo: UniNamespace.GetDeviceInfoResult;
	/** 窗口信息 */
	windowInfo: UniNamespace.GetWindowInfoResult;
	/**
	 * 小程序胶囊按钮信息
	 * 非小程序平台下，只有默认高度
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

/** 应用启动配置与运行时缓存 */
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

		/** 设置非空应用名称。 */
		const setAppName = (appName: string) => {
			if (!appName) return;
			state.appName = appName;
		};

		/** 启动配置就绪后重新加载字典；失败时记录日志。 */
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

		/** 启动配置就绪后更新请求超时和加密选项。 */
		const setFastAxios = () => {
			// 判断是否存在 Launch 数据
			if (!state.hasLaunch) return;
			useFastAxios().setOptions({
				timeout: state.requestTimeout,
				requestCipher: state.requestEncipher,
			});
		};

		/** 加载应用启动数据，并更新导航标题、请求配置和字典；启动请求异常继续向外传播。 */
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

		/**
		 * 获取已加载的字典项；不存在时记录日志并返回 null。
		 *
		 * @param key - 字典名称
		 * @param includeAll - 是否在结果首部增加值为 null 的“全部”选项
		 */
		const getDictionary = (key: string, includeAll = false): FaTableEnumColumnCtx[] => {
			if (!dictionary.value.has(key)) {
				logger.error("app", `字典 [${key}] 不存在`);
				return null;
			}
			const items = dictionary.value.get(key);
			return includeAll ? [{ label: "全部", value: null }, ...items] : items;
		};

		/** 清空 Local 存储并恢复安装标识，不直接重置内存中的 Store。 */
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
