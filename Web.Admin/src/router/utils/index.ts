import { NavigationFailureType, isNavigationFailure } from "vue-router";
import { ElNotification } from "element-plus";
import { randomString } from "@fast-china/utils";
import { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";
import router from "@/router";
import { useUserInfo } from "@/stores";
import { layoutRoute } from "../modules/layoutRoute";
import type { NavigationFailure, RouteLocationNormalized, RouteLocationRaw, RouteRecordRaw, Router } from "vue-router";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";

const modules = import.meta.glob("/src/views/**/*.vue");

/** 复制路由配置，同时保留组件加载函数。 */
const cloneRoute = (route: RouteRecordRaw): RouteRecordRaw => ({
	...route,
	...(route.meta && { meta: { ...route.meta } }),
	...(route.children && { children: route.children.map(cloneRoute) }),
});

/** 加载组件。 */
const loadComponent = (component: string) => {
	if (component) {
		if (component.includes("/")) {
			return modules[`/src/views/${component}.vue`];
		}
		return [`/src/views/${component}/index.vue`];
	} else {
		return () => import("@/views/common/empty/index.vue");
	}
};

/** 加载组件名称。 */
const loadComponentName = (name: string) => {
	if (name) {
		if (name.includes("/")) {
			const cArr = name.replace(/(^|\/)index(?=\/|$)/gi, "").split("/");
			let result = "";
			cArr.forEach((item) => {
				result += item.slice(0, 1).toUpperCase() + item.slice(1);
			});
			return result;
		}
		return name;
	} else {
		return randomString(8);
	}
};

/** 递归组装目录和菜单路由，按钮权限不参与页面路由注册。 */
const packageMenu = (menuList: AuthMenuInfoDto[]) => {
	const routeList: RouteRecordRaw[] = [];

	for (const item of menuList) {
		if ((item.menuType & (MenuTypeEnum.Catalog | MenuTypeEnum.Menu)) === 0) {
			continue;
		}
		const routeInfo: RouteRecordRaw = {
			path: item.menuType === MenuTypeEnum.Catalog ? randomString(8) : item.router,
			// 页签使用路由名称维护 KeepAlive include 列表，因此页面组件 name 必须与此名称一致。
			name: loadComponentName(item.component || item.menuCode),
			component: loadComponent(item.component),
			meta: {
				title: item.menuTitle || item.menuName,
				icon: item.icon,
				tab: item.tab,
				hide: item.visible,
				keepAlive: item.keepAlive,
			},
			children: [],
		};

		// 判断是否存在子节点
		if (item.children && item.children.length > 0) {
			const childrenRoutes = packageMenu(item.children);
			routeInfo.children.push(...childrenRoutes);
			routeInfo.redirect = childrenRoutes[0].path;
			delete routeInfo.component;
			routeInfo.meta.keepAlive = false;
		}

		routeList.push(routeInfo);
	}

	return routeList;
};

/**
 * 根据当前用户菜单重新注册布局路由，替换已有同名路由。
 */
export const handleDynamicRoute = (): void => {
	const userInfoStore = useUserInfo();

	const layoutRouteCopy = cloneRoute(layoutRoute);

	// 组装路由，循环添加到 layout 中
	const layoutRoutes = packageMenu(userInfoStore.menuList);
	layoutRoutes.forEach((rItem) => {
		layoutRouteCopy.children.push(rItem);
	});

	// 尝试移除
	if (router.hasRoute(layoutRouteCopy.name)) {
		router.removeRoute(layoutRouteCopy.name);
	}

	router.addRoute(layoutRouteCopy);
};

/**
 * 路由导航与页签数据处理工具
 */
export const routerUtil = {
	/**
	 * 执行导航并提示已知的导航失败原因；异常提示后继续向调用方抛出。
	 *
	 * @param router - 应用路由实例
	 * @param to - 目标路由，与 router.push 的参数一致
	 * @returns 导航结果；守卫中止或重复导航以 NavigationFailure 返回
	 */
	routePushSafe(router: Router, to: RouteLocationRaw): Promise<NavigationFailure | void> {
		return router
			.push(to)
			.then((failure) => {
				if (failure) {
					if (isNavigationFailure(failure, NavigationFailureType.aborted)) {
						ElNotification({
							message: "导航失败，导航守卫拦截！",
							type: "error",
						});
					} else if (isNavigationFailure(failure, NavigationFailureType.duplicated)) {
						ElNotification({
							message: "导航失败，已在导航目标位置！",
							type: "warning",
						});
					}
				}
				return failure;
			})
			.catch((error: unknown) => {
				ElNotification({
					message: "导航失败，路由无效！",
					type: "error",
				});
				throw error;
			});
	},
	/**
	 * 提取页签需要的路由字段，避免保存组件和匹配记录等运行时对象。
	 */
	pickByRoute(route: Partial<RouteLocationNormalized>): Partial<RouteLocationNormalized> {
		const keys: (keyof RouteLocationNormalized)[] = ["name", "path", "query", "fullPath", "meta", "params"];
		return Object.fromEntries(keys.filter((key) => key in route).map((key) => [key, route[key]]));
	},
	/**
	 * 按父节点优先的顺序展开路由；有子节点的路由复制后移除 children。
	 */
	flattenRoutes(routes: RouteRecordRaw[]): RouteRecordRaw[] {
		const resRoutes: RouteRecordRaw[] = [];

		routes.forEach((item) => {
			if (item?.children?.length > 0) {
				const newItem = { ...item };
				delete newItem?.children;
				resRoutes.push(newItem);
				resRoutes.push(...this.flattenRoutes(item?.children));
			} else {
				resRoutes.push(item);
			}
		});

		return resRoutes;
	},
};
