import { defineStore } from "pinia";
import { reactive, shallowRef, toRefs } from "vue";
import { ElMessageBox } from "element-plus";
import { useFastAxios } from "@fast-china/axios";
import { logger } from "@fast-china/utils";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import { appApi } from "@/api/services/Center/app";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import { useConfig } from "../config";
import type { FaTableColumnCtx, FaTableEnumColumnCtx } from "fast-element-plus";
import type { LaunchOutput } from "@/api/services/Center/app/models/LaunchOutput";

/** 可选的登录页布局名称 */
export type ILoginComponent = "ClassicLogin" | "ModernLogin" | "SimpleLogin" | "SplitLogin";
/** 应用默认主题色 */
export const defaultThemeColor = "#409EFF";

type IState = {
	/** 是否存在 Launch 数据 */
	hasLaunch: boolean;
	/** ICP 备案号 */
	icpSecurityCode: string;
	/** 公安备案号 */
	publicSecurityCode: string;
};

/** 应用启动配置与运行时缓存 */
export const useApp = defineStore(
	"app",
	() => {
		const state = reactive<IState & Required<LaunchOutput>>({
			edition: EditionEnum.None,
			appNo: "",
			appName: "Fast.Admin",
			logoUrl: "",
			themeColor: defaultThemeColor,
			appType: AppEnvironmentEnum.Web,
			environmentType: EnvironmentTypeEnum.Development,
			loginComponent: "ClassicLogin",
			requestTimeout: 6000,
			requestEncipher: true,
			tenantName: "",
			hasLaunch: false,
			icpSecurityCode: "",
			publicSecurityCode: "",
		});

		/** 字典 */
		const dictionary = shallowRef<Map<string, FaTableEnumColumnCtx[]>>(new Map());

		/** 表格列 */
		const tableColumns = shallowRef<Map<string, FaTableColumnCtx[]>>(new Map());

		/** 加载运行配置和启动数据，并更新字典；启动接口失败时允许沿用已有启动缓存。 */
		const launch = async () => {
			try {
				const publicPath = import.meta.env.VITE_PUBLIC_PATH.endsWith("/")
					? import.meta.env.VITE_PUBLIC_PATH
					: `${import.meta.env.VITE_PUBLIC_PATH}/`;
				const appSettingsResponse = await fetch(`${publicPath}appsetting.json`, {
					cache: "no-store",
				});
				if (!appSettingsResponse.ok) {
					throw new Error(`加载 appsetting.json 失败：HTTP ${appSettingsResponse.status}`);
				} else {
					const appSettings: unknown = await appSettingsResponse.json();
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
				}
			} catch (error) {
				logger.error("App", "前端运行配置加载失败", error);
			}

			try {
				const apiRes = await appApi.launch();
				logger.debug("Launch", apiRes);
				Object.assign(state, apiRes);
				state.hasLaunch = true;
			} catch (error) {
				logger.error("App", error);
				// 避免 Launch 接口出现问题，如果存在缓存，也正常进入
				if (!state.hasLaunch) {
					ElMessageBox.alert("系统初始化失败，请稍后刷新浏览器重试。", {
						title: "系统错误",
						type: "error",
						showClose: false,
					});
				}
			} finally {
				state.loginComponent ||= "ClassicLogin";

				// 判断是否存在 Launch 数据
				if (state.hasLaunch) {
					// 使用服务端应用名称更新页面标题。
					document.title = state.appName;

					useFastAxios().setOptions({
						timeout: state.requestTimeout,
						requestCipher: state.requestEncipher,
					});
				}

				try {
					// 处理数据字典
					dictionary.value.clear();
					const _dictionary = await dictionaryApi.queryDictionary();
					Object.entries(_dictionary).forEach(([key, value]) => {
						dictionary.value.set(key, value);
					});
				} catch {
					logger.error("App", "字典加载失败");
				}

				const configStore = useConfig();
				configStore.layout.themeColor ||= state.themeColor || defaultThemeColor;
			}
		};

		/**
		 * 获取已加载的字典项；不存在时返回 null。
		 *
		 * @param key - 字典名称
		 * @param throwError - 是否记录缺失字典的错误日志，不会因此抛出异常
		 */
		const getDictionary = (key: string, throwError = true): FaTableEnumColumnCtx[] => {
			if (!dictionary.value.has(key)) {
				if (throwError) {
					logger.error("app", `字典 [${key}] 不存在`);
				}
				return null;
			}
			return dictionary.value.get(key);
		};

		/**
		 * 获取应用内的表格列缓存；不存在时返回 null。
		 *
		 * @param tableKey - 表格配置标识
		 * @param throwError - 是否记录缓存缺失的错误日志，不会因此抛出异常
		 */
		const getTableColumns = (tableKey: string, throwError = true): FaTableColumnCtx[] => {
			if (!tableColumns.value.has(tableKey)) {
				if (throwError) {
					logger.error("app", `表格列 [${tableKey}] 不存在`);
				}
				return null;
			}
			return tableColumns.value.get(tableKey) ?? [];
		};

		/** 设置或替换指定表格的应用内列缓存。 */
		const setTableColumns = (tableKey: string, columns: FaTableColumnCtx[]) => {
			if (tableColumns.value.has(tableKey)) {
				tableColumns.value.delete(tableKey);
			}
			tableColumns.value.set(tableKey, columns);
		};

		/** 使指定表格的应用内列缓存失效。 */
		const deleteTableColumns = (tableKey: string) => {
			if (tableColumns.value.has(tableKey)) {
				tableColumns.value.delete(tableKey);
			}
		};

		return {
			...toRefs(state),
			launch,
			getDictionary,
			getTableColumns,
			setTableColumns,
			deleteTableColumns,
		};
	},
	{
		persist: {
			key: "store-app",
		},
	}
);
