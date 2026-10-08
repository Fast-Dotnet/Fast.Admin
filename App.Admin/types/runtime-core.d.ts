/**
 * 页面路由扩展配置
 */
interface RouteOptions {
	/**
	 * 页面背景颜色
	 *
	 * @defaultValue "var(--wot-filled-bottom)"
	 */
	backgroundColor?: string;
	/**
	 * 是否显示页脚
	 *
	 * @defaultValue true
	 * @remarks 设置为 `false` 时强制隐藏页脚，优先级最高。
	 */
	footer?: boolean;
	/**
	 * 是否显示页面水印
	 *
	 * @defaultValue true
	 * @remarks 设置为 `false` 时强制隐藏水印，优先级最高。
	 */
	watermark?: boolean;
	/**
	 * 是否为底部导航栏页面
	 *
	 * @defaultValue false
	 * @remarks 设置为 `true` 时显示底部导航栏。
	 */
	isTabBar?: boolean;
	/**
	 * 是否启用页面级滚动
	 *
	 * @defaultValue true
	 * @remarks 设置为 `false` 时页面高度固定，并使用内部滚动区域。
	 */
	pageScroll?: boolean;
	/**
	 * 登录后是否禁止访问当前页面
	 *
	 * @defaultValue false
	 * @remarks 设置为 `true` 时，已登录用户无法进入当前页面，并跳转至工作台。
	 */
	authForbidView?: boolean;
	/**
	 * 是否要求用户已绑定手机号
	 *
	 * @defaultValue false
	 * @remarks 设置为 `true` 时，缺少手机号时阻止本次导航，并设置授权弹窗的显示状态。
	 */
	mobileRequired?: boolean;
	/**
	 * 是否允许未登录访问当前页面
	 *
	 * @defaultValue false
	 * @remarks 设置为 `true` 时，无需登录即可进入当前页面。
	 */
	noLogin?: boolean;
}

declare module "uni-mini-router" {
	// eslint-disable-next-line @typescript-eslint/no-empty-object-type -- 通过模块扩展将页面配置并入路由类型
	interface Route extends RouteOptions {}
}

declare module "@uni-helper/vite-plugin-uni-pages" {
	interface UserPageItem extends RouteOptions {
		/**
		 * 页面使用的布局
		 *
		 * @remarks
		 * - 设置为 `"layout"` 时使用默认布局；
		 * - 设置为 `false` 时禁用布局，页面内容将直接渲染。
		 */
		layout?: "layout" | false;
	}
}

export {};
