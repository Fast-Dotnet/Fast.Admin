/// <reference types="@uni-helper/vite-plugin-uni-pages/client" />
import { getLocalTimeGreeting, logger } from "@fast-china/utils";
import { type RouteRecordRaw, createRouter } from "@wot-ui/router";
import { pages, subPackages } from "virtual:uni-pages";
import { CommonRoute, TabBarRoute } from "@/common";
import { useToast } from "@/hooks";
import { initWebSocket } from "@/signalR";
import { useUserInfo } from "@/stores";

const generateRoutes = (): RouteRecordRaw[] => {
	const mainRoutes = pages.map((page) => ({ ...page, path: `/${page.path}` }));
	const packageRoutes = (subPackages ?? []).flatMap((subPackage) =>
		subPackage.pages.map((page) => ({ ...page, path: `/${subPackage.root}/${page.path}` }))
	);
	return [...mainRoutes, ...packageRoutes] as RouteRecordRaw[];
};

const router = createRouter({ routes: generateRoutes() });

const defaultRoutePath = new Set([CommonRoute.Launcher, CommonRoute.Login, CommonRoute.WebView]);

/** 只接受以单个斜杠开头的站内路径，排除协议相对地址。 */
const getLoginRedirect = (value: unknown) => {
	const redirect = Array.isArray(value) ? value[0] : value;
	return typeof redirect === "string" && redirect.startsWith("/") && !redirect.startsWith("//") ? redirect : "";
};

router.beforeEach(async (to, from, next) => {
	const userInfoStore = useUserInfo();

	// 判断是否存在Token
	if (!userInfoStore.token) {
		// 判断当前页面是否需要登录
		if (!to.noLogin) {
			useToast.warning("请登录");
			// 如果去的路由和来的路由一致，则携带来的路由的参数
			if (from.path === to.path) {
				next({ path: CommonRoute.Login, navType: "replaceAll", query: from.query });
				return;
			}
			// 如果是默认路由，则不处理重定向
			if (defaultRoutePath.has(to.path)) {
				next({ path: CommonRoute.Login, navType: "replaceAll" });
				return;
			}
			next({ path: CommonRoute.Login, navType: "replaceAll", query: { redirect: encodeURIComponent(to.fullPath) } });
			return;
		}
	} else {
		// 已有 Token 但尚未加载用户信息时，先完成用户状态初始化。
		if (!userInfoStore.hasUserInfo) {
			try {
				// 刷新用户信息
				await userInfoStore.refreshUserInfo();

				// 标记用户信息已加载，避免后续导航重复初始化。
				userInfoStore.hasUserInfo = true;

				// 初始化 WebSocket
				initWebSocket();

				// 延迟 0.5 秒显示欢迎信息
				setTimeout(() => {
					useToast.show({
						msg: `${getLocalTimeGreeting()}${userInfoStore.employeeName}`,
						duration: 1500,
					});
				}, 500);
			} catch (error) {
				logger.error("InitRoute", error);
				// 退出登录
				userInfoStore.logout();
				next(false);
				return;
			}
		}

		// 判断是否存在重定向路径，如果有则跳转
		const redirect = getLoginRedirect(decodeURIComponent(String(to.query.redirect ?? "")));
		if (redirect && redirect !== to.fullPath) {
			// 判断是否为 TabBar 页面
			if (TabBarRoute.some((item) => redirect.startsWith(item))) {
				next({ path: redirect, navType: "pushTab" });
				return;
			} else {
				next({ path: redirect, navType: "replace" });
				return;
			}
		}

		// 判断登录后是否禁止查看该页面
		if (to.authForbidView) {
			// 重定向到工作台
			next({ path: CommonRoute.Workbench, navType: "pushTab" });
			return;
		}
	}

	// 需要手机号授权的页面必须先通过登录授权检查。
	if (to.mobileRequired && !userInfoStore.authMobileCheck()) return;

	next();
});

router.afterEach((to) => {
	const userInfoStore = useUserInfo();

	// 判断进入的页面是否为 TabBar 页面
	if (TabBarRoute.includes(to.path)) userInfoStore.activeTabBar = to.path;
});

export default router;
