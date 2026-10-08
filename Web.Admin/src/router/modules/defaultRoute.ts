import type { RouteRecordRaw } from "vue-router";

/**
 * 登录、错误页面与重定向等静态路由
 */
export const defaultRoute: RouteRecordRaw[] = [
	{
		path: "/login",
		name: "Login",
		component: () => import("@/views/login/index.vue"),
		meta: {
			title: "登录",
			authForbidView: true,
			noLogin: true,
		},
	},
	{
		path: "/403",
		name: "Forbidden",
		component: () => import("@/views/common/403/index.vue"),
		meta: {
			title: "无权限操作",
		},
	},
	{
		path: "/404",
		name: "NotFound",
		component: () => import("@/views/common/404/index.vue"),
		meta: {
			title: "页面找不到了",
		},
	},
	{
		path: "/empty",
		name: "Empty",
		component: () => import("@/views/common/empty/index.vue"),
		meta: {
			title: "空页面",
		},
	},
	{
		path: "/redirect/:path(.*)",
		name: "Redirect",
		component: () => import("@/views/common/redirect/index.vue"),
		meta: {
			title: "重定向",
			noLogin: true,
		},
	},
	{
		path: "/mobileBlocked",
		name: "MobileBlocked",
		component: () => import("@/views/common/mobileBlocked/index.vue"),
		meta: {
			title: "请使用电脑访问",
			noLogin: true,
		},
	},
	{
		path: "/:path(.*)*",
		redirect: "/404",
	},
];
