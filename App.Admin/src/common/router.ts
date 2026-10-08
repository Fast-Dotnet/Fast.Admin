/**
 * 常用路由
 */
export const CommonRoute = {
	/** 应用启动页 */
	Launcher: "/pages/launcher/index",
	/** 内嵌网页页 */
	WebView: "/pages/webView/index",
	/** 隐私协议 */
	PrivacyAgreement: "/pages/agreement/privacy/index",
	/** 服务协议 */
	ServiceAgreement: "/pages/agreement/service/index",
	/** 投诉建议列表页 */
	ComplaintPage: "/pages/complaint/page/index",
	/** 投诉建议提交页 */
	ComplaintSubmit: "/pages/complaint/submit/index",
	/** 登录页 */
	Login: "/pages/login/index",
	/** 重置密码页 */
	PasswordReset: "/pages/passwordReset/index",
	/** 首页 */
	Home: "/pages/tabBar/home/index",
	/** 工作台 */
	Workbench: "/pages/tabBar/workbench/index",
	/** 我的 */
	My: "/pages/tabBar/my/index",
};

/**
 * TabBar 路由
 */
export const TabBarRoute = [CommonRoute.Home, CommonRoute.Workbench, CommonRoute.My];
