import { defineStore } from "pinia";
import { reactive, shallowRef, toRefs } from "vue";
import { useFastAxios } from "@fast-china/axios";
import { Local, getOrCreateInstallationId, installationIdentity, logger } from "@fast-china/utils";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
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
			loginComponent: "",
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

		/** 从部署静态目录读取 H5 运行配置；读取失败交由启动入口记录，不阻断启动接口 */
		const loadAppSettings = async (): Promise<void> => {
			// #ifdef H5
			// 清除持久化的旧值，避免配置移除或加载失败后仍显示旧备案号。
			state.icpSecurityCode = "";
			state.publicSecurityCode = "";
			const publicPath = import.meta.env.BASE_URL.endsWith("/") ? import.meta.env.BASE_URL : `${import.meta.env.BASE_URL}/`;
			const url = new URL(`${publicPath}static/appsetting.json`, window.location.origin);
			const response = await fetch(url.href, { cache: "no-store" });
			if (!response.ok) {
				throw new Error(`加载 appsetting.json 失败：HTTP ${response.status}`);
			}
			const appSettings: unknown = await response.json();
			if (typeof appSettings !== "object" || appSettings === null) {
				throw new Error("appsetting.json 必须是 JSON 对象");
			} else {
				const { icpSecurityCode, publicSecurityCode } = appSettings as Record<string, unknown>;
				if (typeof icpSecurityCode === "string" && icpSecurityCode.trim()) {
					state.icpSecurityCode = icpSecurityCode.trim();
				}
				if (typeof publicSecurityCode === "string" && publicSecurityCode.trim()) {
					state.publicSecurityCode = publicSecurityCode.trim();
				}
			}
			// #endif
		};

		/** 设置非空应用名称。 */
		const setAppName = (appName: string) => {
			if (!appName) return;
			state.appName = appName;
		};

		/** 启动配置就绪后重新加载字典；失败时记录日志。 */
		const setDictionary = async () => {
			try {
				// 处理数据字典
				const _dictionary = await dictionaryApi.queryDictionary();
				// 整体替换以通知 shallowRef 的订阅组件，加载失败时保留原字典。
				dictionary.value = new Map(Object.entries(_dictionary));
			} catch (error) {
				logger.error("App", "字典加载失败", error);
			}
		};

		/** 使用缓存或默认配置更新请求超时和加密选项。 */
		const setFastAxios = () => {
			useFastAxios().setOptions({
				timeout: state.requestTimeout,
				requestCipher: state.requestEncipher,
			});
		};

		/**
		 * 获取已加载的字典项；不存在时记录日志并返回 null。
		 *
		 * @typeParam T - 字典项类型，默认为 FaTableEnumColumnCtx
		 * @param key - 字典名称
		 * @param includeAll - 是否在结果首部增加值为 null 的“全部”选项
		 * @returns 字典项数组；字典不存在时返回 null
		 *
		 * @remarks
		 * 泛型仅指定返回类型，不转换实际数据结构。
		 * includeAll 为 true 时，目标类型需要能够表示值为 null 的“全部”选项。
		 */
		const getDictionary = <T = FaTableEnumColumnCtx>(key: string, includeAll = false): T[] | null => {
			if (!dictionary.value.has(key)) {
				logger.error("app", `字典 [${key}] 不存在`);
				return null;
			}
			const items = dictionary.value.get(key);
			const result = includeAll ? [{ label: "全部", value: null }, ...items] : items;

			return result as unknown as T[];
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
			loadAppSettings,
			setAppName,
			setDictionary,
			setFastAxios,
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
