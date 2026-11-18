import { axiosUtil } from "@fast-china/axios";
import { isMobileUserAgent, isTabletUserAgent, logger } from "@fast-china/utils";

// 本次页面加载共用的跳转检查任务
let redirectTask: Promise<boolean> | undefined;

/**
 * 根据访问设备在 Web 与 H5 之间自动跳转。
 *
 * @remarks
 * 仅在 VITE_ENABLE_BROWSER_REDIRECT 为 true 时执行检查。
 * 本次页面加载期间复用首次检查任务，并发调用共享同一个 Promise。
 * 显式选择当前端、设备与当前端匹配、未找到可用目标或检查失败时，返回 false，由启动入口继续加载当前站点。
 * 目标地址沿用当前页面的协议，路由按域名根目录拼接。
 *
 * @param client - 当前站点类型
 * @returns 是否已发起跨端跳转
 */
export function tryBrowserRedirect(client: "Web" | "H5"): Promise<boolean> {
	if (import.meta.env.VITE_ENABLE_BROWSER_REDIRECT !== "true") return Promise.resolve(false);

	return (redirectTask ??= resolveBrowserRedirect(client));
}

/** 按设备类型查询对应页面，校验目标地址后发起跳转。 */
async function resolveBrowserRedirect(client: "Web" | "H5"): Promise<boolean> {
	try {
		const current = new URL(window.location.href);

		// 尊重用户显式选择的客户端，自动跳转后也通过该参数避免再次切换。
		if (current.searchParams.get("__client") === client) return false;

		// 手机和平板优先使用 H5，桌面浏览器优先使用 Web。
		const mobile = isMobileUserAgent() || isTabletUserAgent();
		if ((client === "Web" && !mobile) || (client === "H5" && mobile)) return false;

		// 仅用站内路径匹配页面，不透传当前地址的查询参数和片段。
		const result = await axiosUtil.request<{
			/** 目标站点域名，可包含端口 */
			host: string;
			/** 目标站内路由，以单个 / 开头 */
			path: string;
		}>({
			url: "/queryBrowserRedirect",
			method: "get",
			params: { path: current.pathname.replace(/\/+$/, "") || "/" },
			requestType: "query",
			timeout: 3000,
			showErrorMessage: false,
			showCodeMessage: false,
		});
		if (!result) return false;

		const target = new URL(`${current.protocol}//${result.host}/`);

		// 域名解析结果须与接口返回一致，并禁止在地址中携带用户名或密码。
		if (target.host !== result.host || target.username || target.password) return false;
		// 仅接受不含反斜杠、查询参数和片段的站内绝对路径。
		if (!result.path.startsWith("/") || result.path.startsWith("//") || /[\\?#]/.test(result.path)) return false;
		// 同源目标继续使用当前站点，避免重复加载。
		if (target.origin === current.origin) return false;

		target.pathname = result.path;
		target.searchParams.set("__client", client === "Web" ? "H5" : "Web");

		// 替换当前历史记录，避免返回时重新触发入口跳转。
		window.location.replace(target.href);
		return true;
	} catch {
		logger.warn("BrowserRedirect", "查询目标站点失败，继续加载当前站点");
		return false;
	}
}
