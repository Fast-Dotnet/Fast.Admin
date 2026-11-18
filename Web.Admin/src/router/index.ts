import { createRouter, createWebHistory } from "vue-router";
import { ElMessage, ElNotification } from "element-plus";
import { getLocalTimeGreeting, isMobileUserAgent, isTabletUserAgent, logger } from "@fast-china/utils";
import NProgress from "nprogress";
import { initWebSocket } from "@/signalR";
import { useApp, useUserInfo } from "@/stores";
import { defaultRoute } from "./modules/defaultRoute";
import { handleDynamicRoute } from "./utils";
if (import.meta.env.DEV) {
	await import("nprogress/nprogress.css");
}

const router = createRouter({
	history: createWebHistory(import.meta.env.VITE_PUBLIC_PATH),
	routes: defaultRoute,
});

const defaultRoutePath = defaultRoute.map((m) => m.path);

/** 获取登录后的站内重定向地址 */
const getLoginRedirect = (value: unknown) => {
	const redirect = Array.isArray(value) ? value[0] : value;
	return typeof redirect === "string" && redirect.startsWith("/") && !redirect.startsWith("//") ? redirect : "";
};

/** 配置 NProgress */
NProgress.configure({
	// 动画方式
	easing: "ease",
	// 递增进度条的速度
	speed: 500,
	// 是否显示加载ico
	showSpinner: true,
	// 自动递增间隔
	trickleSpeed: 200,
	// 初始化时的最小百分比
	minimum: 0.3,
});

/** 路由加载前 */
router.beforeEach(async (to, from) => {
	// 开启进度条
	NProgress.start();

	const isMobileDevice = isMobileUserAgent() || isTabletUserAgent();
	if (to.path === "/mobileBlocked" && !isMobileDevice) {
		return { path: "/", replace: true };
	}

	if (import.meta.env.VITE_ENABLE_MOBILE !== "true" && to.path !== "/mobileBlocked" && isMobileDevice) {
		return "/mobileBlocked";
	}

	const appStore = useApp();
	const userInfoStore = useUserInfo();

	// 判断是否存在Token
	if (!userInfoStore.token) {
		// 判断当前页面是否需要登录
		if (!to.meta.noLogin) {
			ElMessage.warning("请登录");
			// 如果去的路由和来的路由一致，则携带来的路由的参数
			if (from.path === to.path) {
				return { path: "/login", query: from.query };
			}
			// 动态菜单注册前可能先重定向到 404，按原始目标判断是否保留登录后的返回地址。
			if (defaultRoutePath.includes(to.redirectedFrom?.path ?? to.path)) {
				return { path: "/login" };
			}
			return { path: "/login", query: { redirect: to.redirectedFrom?.fullPath ?? to.fullPath } };
		}
	} else {
		// 判断 pinia 中的动态路由生成的状态，必须存在Token才加载
		if (!userInfoStore.asyncRouterGen) {
			try {
				// 刷新用户信息
				await userInfoStore.refreshUserInfo();

				// 加载动态路由
				handleDynamicRoute();

				// 确保路由添加完成
				userInfoStore.asyncRouterGen = true;

				// 初始化 WebSocket
				initWebSocket();

				// 延迟 0.5 秒显示欢迎信息
				setTimeout(() => {
					ElNotification({
						title: "欢迎",
						message: `${getLocalTimeGreeting()}${userInfoStore.employeeName}`,
						type: "success",
						duration: 1500,
					});
				}, 500);

				// 由于新添加的路由在本次不存在，所以进行重定向
				return { ...(to.redirectedFrom ?? to), replace: true };
			} catch (error) {
				logger.error("InitRoute", error);
				// 退出登录
				userInfoStore.logout();
				return false;
			}
		}

		// 判断是否存在重定向路径，如果有则跳转
		const redirect = getLoginRedirect(from.query.redirect);
		if (redirect) {
			const redirectRoute = router.resolve(redirect);
			const isCurrentRedirect = redirectRoute.fullPath === to.fullPath || redirectRoute.fullPath === to.redirectedFrom?.fullPath;
			if (!isCurrentRedirect) {
				// 设置 replace: true，因此导航将不会留下历史记录
				return {
					path: redirectRoute.path,
					query: redirectRoute.query,
					hash: redirectRoute.hash,
					replace: true,
				};
			}
		}

		// 判断登录后是否禁止查看该页面
		if (to.meta.authForbidView) {
			// 重定向到首页
			return { path: "/" };
		}
	}

	// 刷新页面标题
	if (to.meta.title) {
		document.title = `${to.meta.title} - ${userInfoStore.employeeName && `${userInfoStore.employeeName} - `}${userInfoStore.tenantName || appStore.appName}`;
	} else {
		document.title = `${userInfoStore.employeeName && `${userInfoStore.employeeName} - `}${userInfoStore.tenantName || appStore.appName}`;
	}

	return true;
});

/** 路由加载后 */
router.afterEach(() => {
	NProgress.done();
});

export default router;

export * from "./utils";
