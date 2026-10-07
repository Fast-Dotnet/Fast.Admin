import { defineStore } from "pinia";
import { reactive, toRefs } from "vue";
import { withDefineType } from "@fast-china/utils";
import router, { routerUtil } from "@/router";
import type { RouteLocationNormalized, Router } from "vue-router";

/** 导航页签保留的路由信息 */
export type INavTab = Partial<Pick<RouteLocationNormalized, "name" | "path" | "query" | "fullPath" | "meta" | "params">>;

export const useNavTabs = defineStore(
	"navTabs",
	() => {
		const state = reactive({
			/** 当前激活页签索引 */
			activeIndex: -1,
			/** 上一个激活页签索引 */
			lastActiveIndex: -1,
			/** 当前激活页签 */
			activeTab: withDefineType<INavTab>(null),
			/** 导航页签列表 */
			navTabs: withDefineType<INavTab[]>([]),
			/** KeepAlive 缓存的组件名称列表 */
			keepAliveComponentNameList: withDefineType<string[]>([]),
			/** 内容区放大 */
			contentLarge: false,
			/** 当前页签内容是否全屏 */
			contentFull: false,
		});

		// 重置全部页签状态
		const $reset = () => {
			state.activeIndex = -1;
			state.lastActiveIndex = -1;
			state.activeTab = null;
			state.navTabs = [];
			state.keepAliveComponentNameList = [];
			state.contentLarge = false;
			state.contentFull = false;
		};

		// 移除当前页面缓存并通过重定向重新加载页签
		const refreshTab = (route: INavTab) => {
			const fIdx = state.keepAliveComponentNameList.findIndex((f) => f === route.name.toString());
			if (fIdx >= 0) {
				state.keepAliveComponentNameList.splice(fIdx, 1);
			}
			routerUtil.routePushSafe(router, { path: `/redirect${route.path}`, query: route.query });
		};

		// 添加或更新页签，并同步 KeepAlive 缓存
		const addTab = (route: INavTab) => {
			if (route.meta?.tab === false) return;
			const fRouteIdx = state.navTabs.findIndex((f) => f.path === route.path);
			// 不存在时新增页签，已存在时更新对应路由信息
			if (fRouteIdx === -1) {
				state.navTabs.push(routerUtil.pickByRoute(route));
				if (route.meta.keepAlive !== false) {
					if (!state.keepAliveComponentNameList.includes(route.name.toString())) {
						state.keepAliveComponentNameList.push(route.name.toString());
					}
				} else {
					const fIdx = state.keepAliveComponentNameList.findIndex((f) => f === route.name.toString());
					if (fIdx >= 0) {
						state.keepAliveComponentNameList.splice(fIdx, 1);
					}
				}
			} else {
				state.navTabs[fRouteIdx] = routerUtil.pickByRoute(route);
				if (route.meta.keepAlive !== false) {
					if (!state.keepAliveComponentNameList.includes(route.name.toString())) {
						state.keepAliveComponentNameList.push(route.name.toString());
					}
				} else {
					const fIdx = state.keepAliveComponentNameList.findIndex((f) => f === route.name.toString());
					if (fIdx >= 0) {
						state.keepAliveComponentNameList.splice(fIdx, 1);
					}
				}
			}
		};

		// 导航到最后一个页签；无页签时返回首页
		const toLastTab = () => {
			const lastTab = state.navTabs.slice(-1)[0];
			if (lastTab) {
				router.push(lastTab?.fullPath ?? lastTab?.path);
			} else {
				router.push({ path: "/" });
			}
		};

		// 关闭非固定页签并导航到合适的剩余页签
		const closeTab = (route: INavTab) => {
			if (route?.meta?.affix === true) return;
			const findIndex = state.navTabs.findIndex((f) => f.path === route.path);
			if (findIndex >= 0) {
				state.navTabs.splice(findIndex, 1);
			}
			const fIdx = state.keepAliveComponentNameList.findIndex((f) => f === route.name.toString());
			if (fIdx >= 0) {
				state.keepAliveComponentNameList.splice(fIdx, 1);
			}
			if (state.lastActiveIndex !== -1 && state.lastActiveIndex !== state.activeIndex && state.lastActiveIndex < state.navTabs.length) {
				const lastTab = state.navTabs[state.lastActiveIndex];
				router.push(lastTab?.fullPath ?? lastTab?.path);
			} else {
				toLastTab();
			}
		};

		/**
		 * 批量关闭页签，并始终保留固定页签
		 * @param retainRoute - 需要保留的路由，传入 false 时仅保留固定页签
		 * @param direction - 相对于保留路由关闭的方向，传入 false 时仅保留该路由
		 */
		const closeTabs = (retainRoute: INavTab | false = false, direction: "left" | "right" | false = false) => {
			const affixNavTabs = state.navTabs.filter((f) => f?.meta?.affix === true);
			if (retainRoute) {
				const retainRouteIndex = state.navTabs.findIndex((f) => f.path === retainRoute.path);
				let newTabs: INavTab[] = [];
				if (retainRouteIndex === -1) {
					state.navTabs = [...affixNavTabs];
					state.keepAliveComponentNameList = affixNavTabs.filter((f) => f?.meta?.keepAlive !== false).map((m) => m.name.toString());
				} else if (direction === "left") {
					newTabs = [...affixNavTabs, ...state.navTabs.slice(retainRouteIndex)];
				} else if (direction === "right") {
					newTabs = [...affixNavTabs, ...state.navTabs.slice(0, retainRouteIndex + 1)];
				} else {
					newTabs = [...affixNavTabs, retainRoute];
				}

				const _newTabs = newTabs.filter((item, idx, arr) => arr.findIndex((f) => f.path === item.path) === idx);
				state.navTabs = _newTabs;
				state.keepAliveComponentNameList = _newTabs.filter((f) => f?.meta?.keepAlive !== false).map((m) => m.name.toString());
			} else {
				state.navTabs = [...affixNavTabs];
				state.keepAliveComponentNameList = affixNavTabs.filter((f) => f?.meta?.keepAlive !== false).map((m) => m.name.toString());
			}
			toLastTab();
		};

		// 更新当前和上一个激活页签
		const setActiveRoute = (route: INavTab) => {
			const fIdx = state.navTabs.findIndex((f) => f.path === route.path);
			if (fIdx === -1) return;
			state.activeTab = routerUtil.pickByRoute(route);
			state.lastActiveIndex = state.activeIndex;
			state.activeIndex = fIdx;
		};

		// 设置内容区放大状态
		const setContentLarge = (contentLarge: boolean) => {
			state.contentLarge = contentLarge;
		};

		// 设置当前页签内容全屏状态
		const setContentFull = (contentFull: boolean) => {
			state.contentFull = contentFull;
		};

		// 合并布局路由中的固定页签与已持久化的普通页签
		const initNavTabs = (router: Router) => {
			// 仅从 layout 的可见子路由中收集页签
			const allRoutes = router
				.getRoutes()
				.find((f) => f.name === "layout")
				?.children.filter((f) => !f.meta.hide);

			// 扁平化嵌套路由后提取固定页签
			const flRoutes = routerUtil.flattenRoutes(allRoutes);

			const affixNavTabs = flRoutes.filter((f) => f.meta?.affix);
			affixNavTabs.forEach((item) => {
				if (!state.keepAliveComponentNameList.includes(item.name.toString())) {
					state.keepAliveComponentNameList.push(item.name.toString());
				}
			});

			const oldNavTabs = state.navTabs.filter((f) => !f.meta?.affix);
			oldNavTabs.forEach((item) => {
				if (!state.keepAliveComponentNameList.includes(item.name.toString())) {
					state.keepAliveComponentNameList.push(item.name.toString());
				}
			});

			state.navTabs = [...affixNavTabs, ...oldNavTabs];
		};

		return {
			...toRefs(state),
			$reset,
			refreshTab,
			addTab,
			closeTab,
			closeTabs,
			setActiveRoute,
			setContentLarge,
			setContentFull,
			initNavTabs,
		};
	},
	{
		persist: {
			key: "store-nav-tabs",
		},
	}
);
