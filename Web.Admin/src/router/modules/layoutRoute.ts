import type { RouteRecordRaw } from "vue-router";

/**
 * 动态菜单共用的布局路由模板
 *
 * @remarks
 * 注册前复制此模板；固定的 name 用于替换已有布局路由，避免重复注册。
 */
export const layoutRoute: RouteRecordRaw = {
	path: "/",
	name: "layout",
	component: () => import("@/layouts/indexAsync.vue"),
	// 重定向到首页
	redirect: "/dashboard",
	children: [
		{
			path: "/dashboard",
			name: "Dashboard",
			component: () => import("@/views/dashboard/index.vue"),
			meta: {
				title: "首页",
				icon: "fa-icon-Dashboard",
				affix: true,
			},
		},
		{
			path: "/iframe",
			name: "Iframe",
			component: () => import("@/views/common/iframe/index.vue"),
			meta: {
				tab: false,
				hide: true,
				keepAlive: false,
			},
		},
	],
};
