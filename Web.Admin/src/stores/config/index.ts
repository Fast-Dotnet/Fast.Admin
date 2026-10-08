import { defineStore } from "pinia";
import { reactive } from "vue";
import { ElMessage } from "element-plus";
import { mixHexColors, parseHexColor, withDefineType } from "@fast-china/utils";
import { useApp } from "../app";
import type { componentSizes } from "element-plus";
import type { FaTableDataRange } from "fast-element-plus";

/** 布局模式 */
export type IModeName = "Classic" | "Horizontal" | "Mixed";

/** 导航页签样式 */
export type INavTabStyle = "Smart" | "Card" | "Chrome";

/** 页面切换动画名称 */
export type IAnimationName =
	| "slide-right"
	| "slide-left"
	| "slide-bottom"
	| "slide-top"
	| "el-fade-in-linear"
	| "el-fade-in"
	| "el-zoom-in-center"
	| "el-zoom-in-top"
	| "el-zoom-in-bottom";

const defaultSize: (typeof componentSizes)[number] = "default";
const defaultMode: IModeName = "Classic";
const defaultNavTab: INavTabStyle = "Smart";
const defaultAnimation: IAnimationName = "slide-right";

const defaultLayoutSize = {
	navBarHeight: 45,
	navTabHeight: 35,
	menuWidth: 180,
	menuHeight: 50,
	mainPadding: 5,
	footerHeight: 30,
};

const smallLayoutSize = {
	navBarHeight: 40,
	navTabHeight: 30,
	menuWidth: 160,
	menuHeight: 45,
	mainPadding: 3,
	footerHeight: 25,
};

/** 布局、主题与表格偏好配置 */
export const useConfig = defineStore(
	"config",
	() => {
		/** 布局配置 */
		const layout = reactive({
			/** 跟随分辨率自动切换大小 */
			autoSize: true,
			/** 布局大小 */
			layoutSize: withDefineType<(typeof componentSizes)[number]>(defaultSize),
			/** 布局方式 */
			layoutMode: withDefineType<IModeName>(defaultMode),
			/** 页签样式 */
			navTabStyle: withDefineType<INavTabStyle>(defaultNavTab),
			/** 切换动画 */
			mainAnimation: withDefineType<IAnimationName>(defaultAnimation),
			/** 主题颜色 */
			themeColor: "",
			/** 导航栏高度 */
			navBarHeight: 45,
			/** 页签高度 */
			navTabHeight: 35,
			/** 菜单折叠 */
			menuCollapse: false,
			/** 菜单宽度 */
			menuWidth: 180,
			/** 菜单高度 */
			menuHeight: 50,
			/** 主页面内容 padding */
			mainPadding: 5,
			/** 页脚高度 */
			footerHeight: 30,
			/** 跟随系统设置，自动切换浅色/深色模式 */
			autoThemMode: true,
			/** 是否深色模式 */
			isDark: false,
			/** 是否置灰模式 */
			isGrey: false,
			/** 是否色弱模式 */
			isWeak: false,
			/** 是否显示页签 */
			navTab: true,
			/** 是否显示面包屑 */
			breadcrumb: true,
			/** 是否显示菜单搜索 */
			menuSearch: true,
			/** 是否显示全屏入口 */
			screenFull: true,
			/** 是否显示页脚 */
			footer: true,
			/** 是否显示水印 */
			watermark: true,
		});

		/** 表格配置 */
		const tableLayout = reactive({
			/** 是否显示表格搜索区域 */
			showSearch: true,
			/** 是否使用抽屉式高级搜索 */
			advancedSearchDrawer: false,
			/** 高级搜索是否默认折叠 */
			defaultCollapsedSearch: true,
			/** 是否默认隐藏表格图片 */
			hideImage: true,
			/** 默认时间搜索范围 */
			dataSearchRange: withDefineType<FaTableDataRange>("Past3D"),
		});

		/** 屏幕 */
		const screen = reactive({
			/** 锁屏密码 */
			password: "",
			/** 屏幕锁定 */
			screenLock: false,
		});

		/** 更新布局模式配置。 */
		const setLayoutMode = (mode: IModeName) => {
			// 暂且这里只赋值
			layout.layoutMode = mode;
		};

		/** 应用主题色；传入空值时恢复应用主题色并显示提示。 */
		const setTheme = (color: string) => {
			if (!color) {
				color = useApp().themeColor;
				ElMessage({ type: "success", message: `主题颜色已重置为 ${color}` });
			}
			const html = document.documentElement;
			const { red, green, blue } = parseHexColor(color);
			html.style.setProperty("--el-color-primary", color);
			html.style.setProperty("--el-color-primary-rgb", `${red}, ${green}, ${blue}`);

			// Element Plus 深色模式的 light 色阶向页面底色混合，浅色模式向白色混合
			const lightMixColor = layout.isDark ? "#141414" : "#ffffff";
			for (let i = 1; i <= 9; i++) {
				html.style.setProperty(`--el-color-primary-light-${i}`, mixHexColors(color, lightMixColor, i / 10));
			}

			// Element Plus 只定义 dark-2：浅色模式压暗，深色模式提亮
			const darkMixColor = layout.isDark ? "#ffffff" : "#000000";
			html.style.setProperty("--el-color-primary-dark-2", mixHexColors(color, darkMixColor, 0.2));
			layout.themeColor = color;
		};

		const darkModeMediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
		let themeMediaListening = false;

		/** 按当前 isDark 配置应用主题，不在此方法中反转配置值。 */
		const switchDark = () => {
			const html = document.documentElement;
			if (layout.isDark) {
				html.classList.add("dark");
			} else {
				html.classList.remove("dark");
			}
			setTheme(layout.themeColor);
		};

		/** 开启跟随系统时读取系统主题并应用。 */
		const switchAutoThemMode = () => {
			if (!layout.autoThemMode) return;
			// 判断是否启用深色模式
			if (darkModeMediaQuery.matches) {
				layout.isDark = true;
			} else {
				layout.isDark = false;
			}
			switchDark();
		};

		/** 应用置灰或色弱显示配置。 */
		const switchGreyOrWeak = (type: "grey" | "weak", value: boolean) => {
			const body = document.body;
			if (!value) {
				body.removeAttribute("style");
				return;
			}
			const styles: Record<"grey" | "weak", string> = {
				grey: "filter: grayscale(1)",
				weak: "filter: invert(80%)",
			};
			body.setAttribute("style", styles[type]);
			layout.isGrey = type === "grey";
			layout.isWeak = type === "weak";
		};

		/** 应用当前主题配置，并确保系统主题变化监听只注册一次。 */
		const initTheme = () => {
			switchAutoThemMode();
			if (!themeMediaListening) {
				darkModeMediaQuery.addEventListener("change", switchAutoThemMode);
				themeMediaListening = true;
			}
			switchDark();
			if (layout.isGrey) switchGreyOrWeak("grey", true);
			if (layout.isWeak) switchGreyOrWeak("weak", true);
		};

		/** 恢复默认布局尺寸。 */
		const setDefaultLayoutSize = () => {
			Object.assign(layout, defaultLayoutSize);
		};

		/** 应用紧凑布局尺寸。 */
		const setSmallLayoutSize = () => {
			Object.assign(layout, smallLayoutSize);
		};

		/** 按预设值重置布局和表格偏好，并重新应用主题。 */
		const reset = () => {
			layout.autoSize = true;
			layout.layoutSize = defaultSize;
			layout.menuCollapse = false;
			layout.layoutMode = defaultMode;
			layout.navTabStyle = defaultNavTab;
			layout.mainAnimation = defaultAnimation;
			layout.themeColor = useApp().themeColor;
			layout.autoThemMode = true;
			layout.isDark = false;
			layout.isGrey = false;
			layout.isWeak = false;
			layout.navTab = true;
			layout.breadcrumb = true;
			layout.menuSearch = true;
			layout.screenFull = true;
			layout.footer = true;
			layout.watermark = true;
			setDefaultLayoutSize();
			initTheme();
			tableLayout.showSearch = true;
			tableLayout.advancedSearchDrawer = false;
			tableLayout.defaultCollapsedSearch = true;
			tableLayout.hideImage = true;
			tableLayout.dataSearchRange = "Past3D";
		};

		return {
			layout,
			tableLayout,
			screen,
			setLayoutMode,
			setTheme,
			switchAutoThemMode,
			switchDark,
			switchGreyOrWeak,
			initTheme,
			setDefaultLayoutSize,
			setSmallLayoutSize,
			reset,
		};
	},
	{
		persist: {
			key: "store-config",
		},
	}
);
