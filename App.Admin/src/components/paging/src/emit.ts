/**
 * 分页组件的完整事件定义，z-paging 事件与 2.8.8 运行时保持一致。
 *
 * @remarks 升级 z-paging 时，需复核事件名称、参数顺序和平台限制。
 */
import type { BaseEvent } from "@uni-helper/uni-types";
import type { CSSProperties } from "vue";

/** 基础布局与交互事件 */
export type ZPagingBaseEmit = {
	/** 分页内容高度改变，单位 px */
	contentHeightChanged: [height: number];
	/** 分页组件内容点击 */
	click: [event: BaseEvent];
	/** 列表触摸方向改变，需开启 watchTouchDirectionChange，nvue 无效 */
	touchDirectionChange: [direction: ZPagingEnums.TouchDirection];
	/** 列表滚动方向改变，需开启 watchScrollDirectionChange，页面滚动模式下无效 */
	scrollDirectionChange: [direction: ZPagingEnums.ScrollDirection];
};

/** 数据查询与分页事件 */
export type ZPagingDataEmit = {
	/** 请求分页数据，参数依次为页码、请求条数和触发来源 */
	query: [pageNo: number, pageSize: number, from: ZPagingEnums.QueryFrom];
	/** 分页列表数据改变 */
	listChange: [list: unknown[]];
	/** 分页请求失败状态改变 */
	isLoadFailedChange: [isLoadFailed: boolean];
};

/** nvue 列表专用事件 */
export type ZPagingNvueEmit = {
	/** 内置列表滚动结束，仅 nvue 有效 */
	scrollend: [event: ZPagingParams.ScrollendEvent];
};

/** 空数据与加载失败视图事件 */
export type ZPagingEmptyViewEmit = {
	/** 点击空态的重新加载按钮，同步调用回调并传入 false 可阻止默认重载 */
	emptyViewReload: [handleReload: (reload?: boolean) => void];
	/** 点击空态视图 */
	emptyViewClick: [];
};

/** 下拉刷新与下拉二楼事件 */
export type ZPagingRefresherEmit = {
	/** 自定义下拉刷新状态改变 */
	refresherStatusChange: [status: ZPagingEnums.RefresherStatus];
	/** 兼容旧版 refresher-status.sync 的状态更新 */
	"update:refresherStatus": [status: ZPagingEnums.RefresherStatus];
	/** 开始下拉时的触点纵坐标，单位 px，nvue 无效 */
	refresherTouchstart: [y: number];
	/** 下拉拖动进度，微信小程序等平台需开启 watchRefresherTouchmove */
	refresherTouchmove: [info: ZPagingParams.RefresherTouchmoveInfo];
	/** 松手时列表内容的下移距离，单位 px，nvue 无效 */
	refresherTouchend: [distance: number];
	/** 下拉二楼开启或关闭 */
	refresherF2Change: [status: ZPagingEnums.GoF2Status];
	/** 下拉刷新触发 */
	onRefresh: [];
	/** 下拉刷新视图复位 */
	onRestore: [];
};

/** 底部加载更多事件 */
export type ZPagingLoadMoreEmit = {
	/** 底部加载更多状态改变 */
	loadingStatusChange: [status: ZPagingEnums.LoadMoreStatus];
};

/** 聊天记录模式事件 */
export type ZPagingChatRecordEmit = {
	/** 聊天列表项的旋转样式改变 */
	cellStyleChange: [style: CSSProperties];
	/** 兼容旧版 cell-style.sync 的样式更新 */
	"update:cellStyle": [style: CSSProperties];
	/** 触摸列表时收起键盘，nvue 无效 */
	hidedKeyboard: [];
	/** 聊天记录模式下的键盘高度改变，仅在支持键盘高度监听的平台触发 */
	keyboardHeightChange: [info: ZPagingParams.KeyboardHeightInfo];
};

/** 列表滚动事件 */
export type ZPagingScrollEmit = {
	/** 列表滚动，事件参数随 Vue 或 nvue 渲染端而异 */
	scroll: [event: ZPagingParams.ScrollInfo | ZPagingParams.ScrollInfoN];
	/** 列表纵向滚动位置改变，单位 px */
	scrollTopChange: [scrollTop: number];
	/** 兼容旧版 scroll-top.sync 的滚动位置更新 */
	"update:scrollTop": [scrollTop: number];
	/** 内置滚动容器滚动到底部 */
	scrolltolower: [];
	/** 内置滚动容器滚动到顶部 */
	scrolltoupper: [];
};

/** 返回顶部事件 */
export type ZPagingBackToTopEmit = {
	/** 点击返回顶部按钮，同步调用回调并传入 false 可阻止默认滚动 */
	backToTopClick: [handleBackToTop: (toTop?: boolean) => void];
};

/** 虚拟列表与内置列表事件 */
export type ZPagingVirtualListEmit = {
	/** 虚拟列表当前渲染的数据改变 */
	virtualListChange: [list: unknown[]];
	/** 兼容旧版 virtualList.sync 的虚拟列表数据更新 */
	"update:virtualList": [list: unknown[]];
	/** 点击列表项，参数依次为数据项和索引 */
	innerCellClick: [item: unknown, index: number];
	/** 虚拟列表顶部占位高度改变，单位 px */
	virtualTopHeightChange: [height: number];
};
