/**
 * z-paging 2.8.8 主组件及其 mixin 的属性定义，使用库内置默认值。
 *
 * @remarks 升级 z-paging 时，需对照新版源码检查属性名称、类型、默认值、校验规则和平台条件。
 * 整组透传给 z-paging 时，显式传入的值会优先于 `uni.$zp` 全局配置。
 */
import type { PropType } from "vue";

/** 可直接显示的文案或按语言提供的文案映射 */
export type I18nText = string | Partial<Record<"en" | "zh-Hans" | "zh-Hant", string>>;

/** 基础布局与通用属性 */
export const zPagingBaseProps = {
	/**
	 * 调用 `complete` 后等待指定时长再处理结果，单位毫秒。
	 * `delay` 大于 0 时优先于 `minDelay`。
	 */
	delay: {
		type: [Number, String],
		default: 0,
	},
	/**
	 * 从触发 `@query` 到处理查询结果的最短时长，单位毫秒。
	 * 例如设为 300，请求用时 100 毫秒时，调用 `complete` 后再等待约 200 毫秒。
	 * 首页 reload 显示下拉刷新视图时，此值至少为 400，且仍受 `delay` 优先级约束。
	 */
	minDelay: {
		type: [Number, String],
		default: 0,
	},
	/**
	 * z-paging 根容器样式
	 * 微信小程序等平台可以通过此属性设置组件根节点样式。
	 */
	pagingStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 根容器类名，样式优先级低于 `pagingStyle` 以及 `height`、`width`、`maxWidth` 和 `bgColor` */
	pagingClass: {
		type: [String, Array, Object] as PropType<string | string[] | Record<string, boolean>>,
		default: "",
	},
	/** 容器高度，支持 `px`、`rpx` 和百分比，优先使用 `pagingStyle.height` */
	height: {
		type: String,
		default: "",
	},
	/** 容器宽度，支持 `px`、`rpx` 和百分比，优先使用 `pagingStyle.width` */
	width: {
		type: String,
		default: "",
	},
	/**
	 * 容器最大宽度，支持 `px`、`rpx` 和百分比。
	 * 留空时铺满可用宽度。设置 `maxWidth` 后自动水平居中，`pagingStyle` 中的最大宽度优先。
	 */
	maxWidth: {
		type: String,
		default: "",
	},
	/** 根容器背景，支持颜色、渐变等 CSS `background` 值，优先使用 `pagingStyle` 中的背景 */
	bgColor: {
		type: String,
		default: "",
	},
	/** 承载内容插槽的父容器样式 */
	pagingContentStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 是否自动计算高度并铺满屏幕 */
	autoHeight: {
		type: Boolean,
		default: false,
	},
	/** 自动高度的增减量，支持 `px` 和 `rpx`，负值用于减少高度 */
	autoHeightAddition: {
		type: [Number, String],
		default: "0px",
	},
	/** 下拉刷新和加载更多的默认主题，支持 black、white */
	defaultThemeStyle: {
		type: String as PropType<"black" | "white">,
		default: "black",
	},
	/**
	 * 使用内置滚动容器时是否启用 `fixed` 布局
	 * 开启时组件默认铺满可用高度，父容器无需设置固定高度。
	 */
	fixed: {
		type: Boolean,
		default: true,
	},
	/**
	 * 是否适配底部安全区域
	 * 使用系统 TabBar 的页面应避免重复开启。
	 */
	safeAreaInsetBottom: {
		type: Boolean,
		default: false,
	},
	/**
	 * 是否在滚动内容末尾预留底部安全区域
	 * 仅在 `safeAreaInsetBottom` 开启时有效。开启后内容滚到底部时才会完全避开安全区域。
	 */
	useSafeAreaPlaceholder: {
		type: Boolean,
		default: false,
	},
	/** 底部区域背景色，可用于设置底部安全区域颜色 */
	bottomBgColor: {
		type: String,
		default: "",
	},
	/** 页面滚动模式下 top 插槽容器的层级 */
	topZIndex: {
		type: Number,
		default: 99,
	},
	/** 内容外层容器的层级 */
	superContentZIndex: {
		type: Number,
		default: 1,
	},
	/** 内容容器的层级 */
	contentZIndex: {
		type: Number,
		default: 1,
	},
	/** 下拉二楼的层级 */
	f2ZIndex: {
		type: Number,
		default: 100,
	},
	/** 页面滚动模式下，内容不足一屏时是否自动撑满 */
	autoFullHeight: {
		type: Boolean,
		default: true,
	},
	/** 是否监听列表触摸方向，nvue 下无效 */
	watchTouchDirectionChange: {
		type: Boolean,
		default: false,
	},
	/** 是否监听列表滚动方向，页面滚动模式下无效 */
	watchScrollDirectionChange: {
		type: Boolean,
		default: false,
	},
	/** 是否仅使用布局，同时关闭自动查询、下拉刷新和加载更多并隐藏空态 */
	layoutOnly: {
		type: Boolean,
		default: false,
	},
	/** 组件布局的默认单位，支持 `rpx` 和 `px` */
	unit: {
		type: String as PropType<"rpx" | "px">,
		default: "rpx",
	},
};

/** 数据查询、分页与缓存属性 */
export const zPagingDataProps = {
	/** 初始页码 */
	defaultPageNo: {
		type: Number,
		default: 1,
	},
	/** 每页条数，必须大于 0，并与实际接口的分页条数一致 */
	defaultPageSize: {
		type: Number,
		default: 10,
		validator: (value: number): boolean => value > 0,
	},
	/**
	 * 当前数据集的标识，可用于区分不同 Tab 的请求。
	 * 通过 `completeByKey` 回填时，标识一致的结果才会被接受。
	 */
	dataKey: {
		type: [Number, String, Object] as PropType<number | string | Record<string, unknown> | null>,
		default: (): number | string | Record<string, unknown> | null => null,
	},
	/** 是否缓存首页数据，开启时需设置 `cacheKey` */
	useCache: {
		type: Boolean,
		default: false,
	},
	/** 缓存键，开启缓存时必填 */
	cacheKey: {
		type: String as PropType<string | null>,
		default: (): string | null => null,
	},
	/** 首页缓存策略，`default` 缓存首次查询结果，`always` 在每次刷新后更新缓存 */
	cacheMode: {
		type: String as PropType<"default" | "always">,
		default: "default",
	},
	/** 自动写入父组件的列表字段名，z-paging 需设置 `ref="paging"` */
	autowireListName: {
		type: String,
		default: "",
	},
	/** 自动调用的父组件查询方法名，z-paging 需设置 `ref="paging"` */
	autowireQueryName: {
		type: String,
		default: "",
	},
	/**
	 * 获取分页数据的回调，设置后由 `fetch` 执行查询。
	 * 默认接收 `pageNo`、`pageSize` 和展开后的 `fetchParams`，拦截器可以替换参数及结果处理方式。
	 */
	fetch: {
		type: Function as PropType<
			| ((
					params: {
						pageNo: number;
						pageSize: number;
					} & Record<string, unknown>
			  ) => unknown)
			| null
		>,
		// Function 类型的 prop 会把函数默认值当作回调，导致 z-paging 跳过 @query。
		// eslint-disable-next-line @typescript-eslint/no-unnecessary-type-assertion -- 关闭 strictNullChecks 时，显式断言可避免 null 被推断为 any（TS7018）
		default: null as null,
	},
	/** 传入 `fetch` 的附加参数，仅在配置 `fetch` 时有效 */
	fetchParams: {
		type: Object as PropType<Record<string, unknown> | null>,
		default: (): Record<string, unknown> | null => null,
	},
	/** 是否在组件挂载后自动查询，设置 `createdReload` 可提前触发 */
	auto: {
		type: Boolean,
		default: true,
	},
	/** 用户下拉刷新时是否触发 reload */
	reloadWhenRefresh: {
		type: Boolean,
		default: true,
	},
	/** reload 时是否自动滚到顶部 */
	autoScrollToTopWhenReload: {
		type: Boolean,
		default: true,
	},
	/** 是否在 `reload` 发起查询时清空旧列表，关闭时在请求返回前保留旧数据 */
	autoCleanListWhenReload: {
		type: Boolean,
		default: true,
	},
	/** reload 时是否显示下拉刷新视图 */
	showRefresherWhenReload: {
		type: Boolean,
		default: false,
	},
	/** reload 时是否显示底部加载中视图 */
	showLoadingMoreWhenReload: {
		type: Boolean,
		default: false,
	},
	/** 是否在 `created` 阶段启动首查，仅在 `auto` 开启时有效 */
	createdReload: {
		type: Boolean,
		default: false,
	},
	/** 本地分页加载更多的延迟，单位毫秒 */
	localPagingLoadingTime: {
		type: [Number, String],
		default: 200,
	},
	/** 是否将 `complete` 回填的数据追加到列表，关闭时覆盖列表，聊天记录模式下无效 */
	concat: {
		type: Boolean,
		default: true,
	},
	/** 请求失败时是否拒绝 `reload`、`complete` 等方法返回的 Promise */
	callNetworkReject: {
		type: Boolean,
		default: true,
	},
};

/** nvue 列表专用属性 */
export const zPagingNvueProps = {
	// #ifdef APP-NVUE
	/** nvue 列表容器类型，支持 list、waterfall、scroller */
	nvueListIs: {
		type: String as PropType<"list" | "waterfall" | "scroller">,
		default: "list",
	},
	/** nvue 瀑布流容器配置，nvueListIs 为 waterfall 时有效 */
	nvueWaterfallConfig: {
		type: Object as PropType<Record<string, unknown>>,
		default: (): Record<string, unknown> => ({}),
	},
	/** nvue 列表是否允许回弹，iOS 不支持动态修改 */
	nvueBounce: {
		type: Boolean,
		default: true,
	},
	/** 通过代码滚到顶部或底部时是否加快动画 */
	nvueFastScroll: {
		type: Boolean,
		default: false,
	},
	/** nvue 列表容器 ID */
	nvueListId: {
		type: String,
		default: "",
	},
	/** nvue 原生 refresh 组件样式 */
	nvueRefresherStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** nvue 列表是否按类似竖向 swiper 的方式分页滚动 */
	nvuePagingEnabled: {
		type: Boolean,
		default: false,
	},
	/** 是否隐藏 nvue 列表底部标记，隐藏后该标记无法触发触底检测 */
	hideNvueBottomTag: {
		type: Boolean,
		default: false,
	},
	/**
	 * nvue 中两次 `scroll` 事件之间的最小滚动距离，单位 px
	 * 值越小，采样越密集。
	 */
	offsetAccuracy: {
		type: Number,
		default: 10,
	},
	// #endif
};

/** 空数据与加载失败视图属性 */
export const zPagingEmptyViewProps = {
	/** 是否强制隐藏空态 */
	hideEmptyView: {
		type: Boolean,
		default: false,
	},
	/** 空态文案，支持语言映射 */
	emptyViewText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 无数据时是否显示重新加载按钮 */
	showEmptyViewReload: {
		type: Boolean,
		default: false,
	},
	/** 加载失败时是否显示重新加载按钮 */
	showEmptyViewReloadWhenError: {
		type: Boolean,
		default: true,
	},
	/** 重新加载按钮文案，支持语言映射 */
	emptyViewReloadText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 空态图片地址，留空时使用内置图片 */
	emptyViewImg: {
		type: String,
		default: "",
	},
	/** 加载失败文案，支持语言映射 */
	emptyViewErrorText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 加载失败图片地址，留空时使用内置图片 */
	emptyViewErrorImg: {
		type: String,
		default: "",
	},
	/** 空态视图样式 */
	emptyViewStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 空态外层容器样式 */
	emptyViewSuperStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 空态图片样式 */
	emptyViewImgStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 空态文案样式 */
	emptyViewTitleStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 重新加载按钮样式 */
	emptyViewReloadStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 是否让空态填充 z-paging 的剩余区域 */
	emptyViewFixed: {
		type: Boolean,
		default: false,
	},
	/** 是否垂直居中显示空态，`emptyViewFixed` 为 `false` 时有效 */
	emptyViewCenter: {
		type: Boolean,
		default: true,
	},
	/** 加载中是否自动隐藏空态 */
	autoHideEmptyViewWhenLoading: {
		type: Boolean,
		default: true,
	},
	/** 用户触发下拉刷新时是否自动隐藏空态 */
	autoHideEmptyViewWhenPull: {
		type: Boolean,
		default: true,
	},
	/** 空态视图的层级 */
	emptyViewZIndex: {
		type: Number,
		default: 9,
	},
};

/** 下拉刷新与下拉二楼属性 */
export const zPagingRefresherProps = {
	/** 下拉刷新主题，支持 `black` 和 `white`，留空时继承 `defaultThemeStyle` */
	refresherThemeStyle: {
		type: String,
		default: "",
	},
	/** 下拉刷新图标样式 */
	refresherImgStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 下拉刷新状态文字样式 */
	refresherTitleStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 最后更新时间的文字样式，仅在 `showRefresherUpdateTime` 开启时有效 */
	refresherUpdateTimeStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 微信和 QQ 小程序中是否实时监听下拉进度 */
	watchRefresherTouchmove: {
		type: Boolean,
		default: false,
	},
	/** 是否仅使用下拉刷新，开启后关闭自动查询和加载更多并隐藏空态 */
	refresherOnly: {
		type: Boolean,
		default: false,
	},
	/** 下拉刷新默认状态回弹时长，单位毫秒，nvue 下无效 */
	refresherDefaultDuration: {
		type: [Number, String],
		default: 100,
	},
	/** 刷新完成后开始回弹前的延迟，单位毫秒 */
	refresherCompleteDelay: {
		type: [Number, String],
		default: 0,
	},
	/**
	 * 刷新结束后的回弹时长，单位毫秒，nvue 下无效。
	 * 关闭 `refresherEndBounceEnabled` 时，实际时长为设定值的三分之一。
	 */
	refresherCompleteDuration: {
		type: [Number, String],
		default: 300,
	},
	/** 刷新中是否允许列表滚动 */
	refresherRefreshingScrollable: {
		type: Boolean,
		default: true,
	},
	/** 刷新结束状态是否允许列表滚动 */
	refresherCompleteScrollable: {
		type: Boolean,
		default: false,
	},
	/** 是否使用 z-paging 的下拉刷新，关闭后使用平台支持的内置 scroll-view 刷新 */
	useCustomRefresher: {
		type: Boolean,
		default: true,
	},
	/** 下拉动画帧率 */
	refresherFps: {
		type: [Number, String],
		default: 40,
	},
	/** 下拉手势触发的角度阈值，单位度，手势角度小于该值时不触发 */
	refresherMaxAngle: {
		type: [Number, String],
		default: 40,
	},
	/** 手势由不满足角度条件变为满足时，是否允许继续下拉 */
	refresherAngleEnableChangeContinued: {
		type: Boolean,
		default: false,
	},
	/** 下拉默认状态文案，支持语言映射 */
	refresherDefaultText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 松手立即刷新文案，支持语言映射 */
	refresherPullingText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 刷新中文案，支持语言映射 */
	refresherRefreshingText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 刷新完成文案，支持语言映射 */
	refresherCompleteText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 继续下拉进入二楼的文案，支持语言映射 */
	refresherGoF2Text: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 下拉默认状态图片 */
	refresherDefaultImg: {
		type: String as PropType<string | null>,
		default: (): string | null => null,
	},
	/** 松手可刷新状态的图片，留空时使用默认状态图片 */
	refresherPullingImg: {
		type: String as PropType<string | null>,
		default: (): string | null => null,
	},
	/** 刷新中状态图片 */
	refresherRefreshingImg: {
		type: String as PropType<string | null>,
		default: (): string | null => null,
	},
	/** 刷新完成状态图片 */
	refresherCompleteImg: {
		type: String as PropType<string | null>,
		default: (): string | null => null,
	},
	/** 刷新中图标是否旋转 */
	refresherRefreshingAnimated: {
		type: Boolean,
		default: true,
	},
	/** 刷新结束时是否允许回弹 */
	refresherEndBounceEnabled: {
		type: Boolean,
		default: true,
	},
	/** 是否启用下拉刷新 */
	refresherEnabled: {
		type: Boolean,
		default: true,
	},
	/** 触发下拉刷新的距离，支持带单位的字符串，数字按 px 计算 */
	refresherThreshold: {
		type: [Number, String],
		default: "80rpx",
	},
	/** 系统下拉刷新样式，支持 black、white、none */
	refresherDefaultStyle: {
		type: String,
		default: "black",
	},
	/** 下拉刷新区域背景色 */
	refresherBackground: {
		type: String,
		default: "transparent",
	},
	/** 固定的下拉刷新区域背景色 */
	refresherFixedBackground: {
		type: String,
		default: "transparent",
	},
	/** 固定下拉刷新背景区域的高度，数字按 px 计算 */
	refresherFixedBacHeight: {
		type: [Number, String],
		default: 0,
	},
	/** 超过下拉阈值后的位移衰减参数，范围 0–1，值越大衰减越明显，nvue 下无效 */
	refresherOutRate: {
		type: Number,
		default: 0.65,
	},
	/** 是否启用下拉进入二楼 */
	refresherF2Enabled: {
		type: Boolean,
		default: false,
	},
	/** 触发进入二楼的距离，支持带单位的字符串，数字按 px 计算 */
	refresherF2Threshold: {
		type: [Number, String],
		default: "200rpx",
	},
	/** 进入二楼的动画时长，单位毫秒 */
	refresherF2Duration: {
		type: [Number, String],
		default: 200,
	},
	/** 进入二楼状态后，松手是否显示二楼 */
	showRefresherF2: {
		type: Boolean,
		default: true,
	},
	/** 列表实际位移与手势下拉距离的比值，nvue 下无效 */
	refresherPullRate: {
		type: Number,
		default: 0.75,
	},
	/** 是否显示最后更新时间 */
	showRefresherUpdateTime: {
		type: Boolean,
		default: false,
	},
	/** 最后更新时间的存储键，列表间需分别记录时使用不同的值 */
	refresherUpdateTimeKey: {
		type: String,
		default: "default",
	},
	/** 达到刷新或二楼阈值时是否短振动，H5 无效 */
	refresherVibrate: {
		type: Boolean,
		default: false,
	},
	/** 是否禁止刷新视图跟随手势移动，刷新逻辑仍会触发 */
	refresherNoTransform: {
		type: Boolean,
		default: false,
	},
	/** 是否为下拉刷新视图预留状态栏高度，适用于隐藏导航栏的页面 */
	useRefresherStatusBarPlaceholder: {
		type: Boolean,
		default: false,
	},
};

/** 底部加载更多属性 */
export const zPagingLoadMoreProps = {
	/** 底部加载更多主题，支持 `black` 和 `white`，留空时继承 `defaultThemeStyle` */
	loadingMoreThemeStyle: {
		type: String,
		default: "",
	},
	/** 加载更多容器样式 */
	loadingMoreCustomStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 加载更多文字样式 */
	loadingMoreTitleCustomStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 加载中图标样式 */
	loadingMoreLoadingIconCustomStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 加载中图标类型，支持 `flower` 和 `circle` */
	loadingMoreLoadingIconType: {
		type: String as PropType<"flower" | "circle">,
		default: "flower",
	},
	/** 自定义加载中图标图片 */
	loadingMoreLoadingIconCustomImage: {
		type: String,
		default: "",
	},
	/** 加载中图标是否旋转 */
	loadingMoreLoadingAnimated: {
		type: Boolean,
		default: true,
	},
	/** 是否允许加载更多，包括滚动触发和点击触发 */
	loadingMoreEnabled: {
		type: Boolean,
		default: true,
	},
	/** 是否允许滚动到底部时自动加载更多 */
	toBottomLoadingMoreEnabled: {
		type: Boolean,
		default: true,
	},
	/**
	 * 是否把默认的加载更多状态显示为加载中。
	 * 开启后，内容不足一屏时不会显示默认的点击加载更多提示。
	 */
	loadingMoreDefaultAsLoading: {
		type: Boolean,
		default: false,
	},
	/** 等待加载更多的文案，支持语言映射 */
	loadingMoreDefaultText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 正在加载更多的文案，支持语言映射 */
	loadingMoreLoadingText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 没有更多数据的文案，支持语言映射 */
	loadingMoreNoMoreText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 加载更多失败的文案，支持语言映射 */
	loadingMoreFailText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
	/** 内容不足一屏时是否隐藏没有更多提示 */
	hideNoMoreInside: {
		type: Boolean,
		default: false,
	},
	/** 列表条数低于此值时隐藏没有更多提示，0 表示不限制 */
	hideNoMoreByLimit: {
		type: Number,
		default: 0,
	},
	/** 是否显示加载更多的默认文案 */
	showDefaultLoadingMoreText: {
		type: Boolean,
		default: true,
	},
	/** 是否显示没有更多数据的视图 */
	showLoadingMoreNoMoreView: {
		type: Boolean,
		default: true,
	},
	/** 是否显示没有更多数据提示两侧的分割线 */
	showLoadingMoreNoMoreLine: {
		type: Boolean,
		default: true,
	},
	/** 没有更多数据提示的分割线样式 */
	loadingMoreNoMoreLineCustomStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 内容不足一屏时是否继续加载更多，nvue 无效 */
	insideMore: {
		type: Boolean,
		default: false,
	},
	/** 距底部或右侧多远时触发 `scrolltolower`，数字按 px 计算 */
	lowerThreshold: {
		type: [Number, String],
		default: "100rpx",
	},
};

/** 加载视图与系统 loading 属性 */
export const zPagingLoadingProps = {
	/** 首次加载完成后是否自动隐藏 loading 插槽 */
	autoHideLoadingAfterFirstLoaded: {
		type: Boolean,
		default: true,
	},
	/** loading 插槽是否固定并铺满屏幕 */
	loadingFullFixed: {
		type: Boolean,
		default: false,
	},
	/** 是否在 `reload` 或 `refresh` 时显示 `uni.showLoading`，下拉刷新和触底加载更多除外 */
	autoShowSystemLoading: {
		type: Boolean,
		default: false,
	},
	/** 系统 loading 是否显示透明遮罩，H5、App、微信小程序和百度小程序有效 */
	systemLoadingMask: {
		type: Boolean,
		default: true,
	},
	/** 系统 loading 文案，支持语言映射 */
	systemLoadingText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: (): I18nText | null => null,
	},
};

/** 聊天记录模式属性 */
export const zPagingChatRecordProps = {
	/** 是否启用聊天记录模式 */
	useChatRecordMode: {
		type: Boolean,
		default: false,
	},
	/** 聊天记录滚到顶部后的列表位移偏移，源码标记为暂未生效 */
	chatRecordMoreOffset: {
		type: [Number, String],
		default: "0rpx",
	},
	/** 触摸聊天列表时是否自动隐藏键盘 */
	autoHideKeyboardWhenChat: {
		type: Boolean,
		default: true,
	},
	/** 聊天记录模式下键盘弹出时是否调整 `bottom` 插槽的占位 */
	autoAdjustPositionWhenChat: {
		type: Boolean,
		default: true,
	},
	/** 键盘弹出时底部占位的高度偏移 */
	chatAdjustPositionOffset: {
		type: String,
		default: "0rpx",
	},
	/** 键盘弹出时是否自动滚到聊天列表底部 */
	autoToBottomWhenChat: {
		type: Boolean,
		default: false,
	},
	/** reload 时是否显示 chatLoading 插槽 */
	showChatLoadingWhenReload: {
		type: Boolean,
		default: false,
	},
	/** 是否把聊天记录顶部的默认加载更多状态显示为加载中 */
	chatLoadingMoreDefaultAsLoading: {
		type: Boolean,
		default: true,
	},
};

/** 列表滚动属性 */
export const zPagingScrollProps = {
	/**
	 * 是否使用页面滚动
	 * 开启时无需给 z-paging 指定滚动容器高度。页面需通过 mixin 或组合式 hook 转发滚动、触底等事件。
	 */
	usePageScroll: {
		type: Boolean,
		default: false,
	},
	/** 是否允许内部容器滚动，适用于内置 scroll-view 和 nvue */
	scrollable: {
		type: Boolean,
		default: true,
	},
	/** 是否显示滚动条 */
	showScrollbar: {
		type: Boolean,
		default: true,
	},
	/** 是否允许横向滚动 */
	scrollX: {
		type: Boolean,
		default: false,
	},
	/** iOS 滚动到顶部时是否允许回弹 */
	scrollToTopBounceEnabled: {
		type: Boolean,
		default: false,
	},
	/** iOS 滚动到底部时是否允许回弹 */
	scrollToBottomBounceEnabled: {
		type: Boolean,
		default: true,
	},
	/** 设置滚动位置时是否使用过渡动画 */
	scrollWithAnimation: {
		type: Boolean,
		default: false,
	},
	/** 要滚动到的子元素 ID，目标方向需开启滚动，ID 不能以数字开头 */
	scrollIntoView: {
		type: String,
		default: "",
	},
	/**
	 * 是否位于 `swiper-item` 等容器中
	 * 用于修复 Vue 3 微信小程序和 QQ 小程序中按 ID 或索引滚动时无法获取节点的问题。
	 */
	inSwiperSlot: {
		type: Boolean,
		default: false,
	},
};

/** 返回顶部属性 */
export const zPagingBackToTopProps = {
	/** 是否在超过滚动阈值时自动显示返回顶部按钮 */
	autoShowBackToTop: {
		type: Boolean,
		default: false,
	},
	/** 显示返回顶部按钮所需的滚动距离，支持带单位的字符串，数字按 px 计算 */
	backToTopThreshold: {
		type: [Number, String],
		default: "400rpx",
	},
	/** 返回顶部按钮图片地址，留空时使用内置图片 */
	backToTopImg: {
		type: String,
		default: "",
	},
	/** 点击返回顶部按钮时是否使用滚动动画 */
	backToTopWithAnimate: {
		type: Boolean,
		default: true,
	},
	/** 返回顶部按钮距底部的距离 */
	backToTopBottom: {
		type: [Number, String],
		default: "160rpx",
	},
	/** 返回顶部按钮样式 */
	backToTopStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 是否允许 iOS 点击状态栏或 Android 双击标题栏返回顶部 */
	enableBackToTop: {
		type: Boolean,
		default: true,
	},
};

/** 虚拟列表与内置列表属性 */
export const zPagingVirtualListProps = {
	/** 是否启用虚拟列表，使用页面滚动或 nvue 时无效 */
	useVirtualList: {
		type: Boolean,
		default: false,
	},
	/** 是否以兼容模式渲染虚拟列表 */
	useCompatibilityMode: {
		type: Boolean,
		default: false,
	},
	/** 虚拟列表兼容模式的附加数据 */
	extraData: {
		type: Object as PropType<Record<string, unknown>>,
		default: (): Record<string, unknown> => ({}),
	},
	/** 是否由 z-paging 循环渲染列表，启用虚拟列表时默认启用 */
	useInnerList: {
		type: Boolean,
		default: false,
	},
	/** 虚拟列表启用时是否关闭自动启用的内置列表，显式设置 `useInnerList` 仍可启用 */
	forceCloseInnerList: {
		type: Boolean,
		default: false,
	},
	/** 内置列表项的 key 字段名，nvue 开启 `useInnerList` 时必填 */
	cellKeyName: {
		type: String,
		default: "",
	},
	/** 内置列表容器样式 */
	innerListStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/** 内置列表项样式 */
	innerCellStyle: {
		type: Object as PropType<Partial<CSSStyleDeclaration>>,
		default: (): Partial<CSSStyleDeclaration> => ({}),
	},
	/**
	 * 可视区域上方和下方各预渲染的屏数，必须大于 0。
	 * 增大此值可以缓解快速滚动时的短暂白屏，同时会增加渲染节点数量。
	 */
	preloadPage: {
		type: [Number, String],
		default: 12,
		validator: (value: number | string): boolean => Number(value) > 0,
	},
	/** 虚拟列表项高度模式，`fixed` 为等高列表，`dynamic` 支持不同项高度 */
	cellHeightMode: {
		type: String as PropType<"fixed" | "dynamic">,
		default: "fixed",
	},
	/** 固定高度模式下显式指定的列表项高度，设置后跳过首项高度测量 */
	fixedCellHeight: {
		type: [Number, String],
		default: 0,
	},
	/** 虚拟列表每行的列数 */
	virtualListCol: {
		type: [Number, String],
		default: 1,
	},
	/** 虚拟列表的滚动采样帧率，过低易出现白屏，过高可能影响性能 */
	virtualScrollFps: {
		type: [Number, String],
		default: 80,
	},
	/** 虚拟列表项 ID 前缀，用于区分同一页面中的多个列表，不能以数字开头 */
	virtualCellIdPrefix: {
		type: String,
		default: "",
	},
	/**
	 * 非内置虚拟列表是否位于 `swiper-item` 等容器中
	 * 仅用于 Vue 3 微信小程序和 QQ 小程序无法测量插槽项高度的情况。
	 */
	virtualInSwiperSlot: {
		type: Boolean,
		default: false,
	},
};
