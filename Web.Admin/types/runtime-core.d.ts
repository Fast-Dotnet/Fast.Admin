declare module "vue-router" {
	interface RouteMeta {
		/**
		 * 设置该路由在侧边栏和面包屑中展示的名字
		 */
		title?: string;
		/**
		 * 是否固定在 Tab 中
		 */
		affix?: boolean;
		/**
		 * 图标
		 */
		icon?: string;
		/**
		 * 是否显示导航页签
		 * @defaultValue true
		 */
		tab?: boolean;
		/**
		 * 登录后是否禁止访问当前页面
		 *
		 * @defaultValue false
		 * @remarks 设置为 true 时，已登录用户会被重定向至首页。
		 */
		authForbidView?: boolean;
		/**
		 * 是否允许未登录访问当前页面
		 * @defaultValue false
		 */
		noLogin?: boolean;
		/**
		 * 是否在侧边栏隐藏路由
		 * @defaultValue false
		 */
		hide?: boolean;
		/**
		 * 是否缓存页面；设为 false 时不加入普通页签的 KeepAlive 列表
		 */
		keepAlive?: boolean;
	}
}

export {};
