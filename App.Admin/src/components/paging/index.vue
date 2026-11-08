<template>
	<z-paging
		v-bind="zPagingProps"
		ref="paging"
		:paging-class="normalizeClass(['z-paging__page', props.pagingClass])"
		:paging-content-style="{ minHeight: '120rpx', ...props.pagingContentStyle }"
		:auto="props.auto ?? false"
		:loading-more-enabled="props.pagination && props.loadingMoreEnabled"
		v-model="modelValue"
		@query="handleQuery"
		@virtual-list-change="handleVirtualListChange"
		@content-height-changed="emit('contentHeightChanged', $event)"
		@tap="emit('click', $event)"
		@touch-direction-change="emit('touchDirectionChange', $event)"
		@scroll-direction-change="emit('scrollDirectionChange', $event)"
		@list-change="emit('listChange', $event)"
		@is-load-failed-change="(isLoadFailed) => emit('isLoadFailedChange', isLoadFailed)"
		@scrollend="emit('scrollend', $event)"
		@empty-view-reload="emit('emptyViewReload', $event)"
		@empty-view-click="emit('emptyViewClick')"
		@refresher-status-change="emit('refresherStatusChange', $event)"
		@update:refresher-status="emit('update:refresherStatus', $event)"
		@refresher-touchstart="emit('refresherTouchstart', $event)"
		@refresher-touchmove="emit('refresherTouchmove', $event)"
		@refresher-touchend="emit('refresherTouchend', $event)"
		@refresher-f2-change="emit('refresherF2Change', $event)"
		@on-refresh="emit('onRefresh')"
		@on-restore="emit('onRestore')"
		@loading-status-change="emit('loadingStatusChange', $event)"
		@cell-style-change="emit('cellStyleChange', $event)"
		@update:cell-style="emit('update:cellStyle', $event)"
		@hided-keyboard="emit('hidedKeyboard')"
		@keyboard-height-change="emit('keyboardHeightChange', $event)"
		@scroll="emit('scroll', $event)"
		@scroll-top-change="emit('scrollTopChange', $event)"
		@update:scroll-top="emit('update:scrollTop', $event)"
		@scrolltolower="emit('scrolltolower')"
		@scrolltoupper="emit('scrolltoupper')"
		@back-to-top-click="emit('backToTopClick', $event)"
		@update:virtual-list="emit('update:virtualList', $event)"
		@inner-cell-click="(item, index?: number) => emit('innerCellClick', item, index)"
		@virtual-top-height-change="emit('virtualTopHeightChange', $event)"
	>
		<slot :paged-list="pagedList" :search-param="searchParam" :set-param="handleSearchParam" :refresh="handleRefresh" />
		<template v-if="slots.top" #top>
			<slot name="top" :paged-list="pagedList" :search-param="searchParam" :set-param="handleSearchParam" :refresh="handleRefresh" />
		</template>
		<template v-if="slots.bottom" #bottom>
			<slot name="bottom" :paged-list="pagedList" :search-param="searchParam" :set-param="handleSearchParam" :refresh="handleRefresh" />
		</template>
		<template v-if="slots.left" #left>
			<slot name="left" :paged-list="pagedList" :search-param="searchParam" :set-param="handleSearchParam" :refresh="handleRefresh" />
		</template>
		<template v-if="slots.right" #right>
			<slot name="right" :paged-list="pagedList" :search-param="searchParam" :set-param="handleSearchParam" :refresh="handleRefresh" />
		</template>

		<template v-if="slots.refresher" #refresher="{ refresherStatus }">
			<slot name="refresher" :refresher-status="refresherStatus" />
		</template>
		<template v-if="slots.refresherComplete" #refresherComplete>
			<slot name="refresherComplete" />
		</template>
		<template v-if="slots.refresherF2" #refresherF2>
			<slot name="refresherF2" />
		</template>
		<template v-if="slots.f2" #f2>
			<slot name="f2" />
		</template>

		<template v-if="slots.loadingMoreDefault" #loadingMoreDefault>
			<slot name="loadingMoreDefault" />
		</template>
		<template v-if="slots.loadingMoreLoading" #loadingMoreLoading>
			<slot name="loadingMoreLoading" />
		</template>
		<template v-if="slots.loadingMoreNoMore" #loadingMoreNoMore>
			<slot name="loadingMoreNoMore" />
		</template>
		<template v-if="slots.loadingMoreFail" #loadingMoreFail>
			<slot name="loadingMoreFail" />
		</template>

		<template v-if="slots.empty" #empty="{ isLoadFailed }">
			<slot name="empty" :is-load-failed="isLoadFailed" />
		</template>
		<template v-if="slots.loading" #loading>
			<slot name="loading" />
		</template>
		<template v-if="slots.backToTop" #backToTop>
			<slot name="backToTop" />
		</template>

		<template v-if="slots.cell" #cell="{ item, index }">
			<slot name="cell" :item="item" :index="index" />
		</template>
		<template v-if="slots.header" #header>
			<slot name="header" />
		</template>
		<template v-if="slots.footer" #footer>
			<slot name="footer" />
		</template>

		<template v-if="slots.chatLoading" #chatLoading="{ loadingMoreStatus }">
			<slot name="chatLoading" :loading-more-status="loadingMoreStatus" />
		</template>
		<template v-if="slots.chatNoMore" #chatNoMore>
			<slot name="chatNoMore" />
		</template>
	</z-paging>
</template>

<script setup lang="ts" generic="TResult, TInput extends PagedInput = PagedInput">
import { onShow } from "@dcloudio/uni-app";
import { computed, nextTick, normalizeClass, shallowRef, watch } from "vue";
import { randomString, useProps } from "@fast-china/utils";
import useZPaging from "z-paging/components/z-paging/js/hooks/useZPaging.js";
import {
	type I18nText,
	zPagingBackToTopProps,
	zPagingBaseProps,
	zPagingChatRecordProps,
	zPagingDataProps,
	zPagingEmptyViewProps,
	zPagingLoadMoreProps,
	zPagingLoadingProps,
	zPagingNvueProps,
	zPagingRefresherProps,
	zPagingScrollProps,
	zPagingVirtualListProps,
} from "./src/props";
import type { ComponentPublicInstance, PropType, VNode } from "vue";
import type {
	ZPagingBackToTopEmit,
	ZPagingBaseEmit,
	ZPagingChatRecordEmit,
	ZPagingDataEmit,
	ZPagingEmptyViewEmit,
	ZPagingLoadMoreEmit,
	ZPagingNvueEmit,
	ZPagingRefresherEmit,
	ZPagingScrollEmit,
	ZPagingVirtualListEmit,
} from "./src/emit";

defineOptions({
	name: "Paging",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...zPagingBaseProps,
	...zPagingDataProps,
	...zPagingNvueProps,
	...zPagingEmptyViewProps,
	...zPagingRefresherProps,
	...zPagingLoadMoreProps,
	...zPagingLoadingProps,
	...zPagingChatRecordProps,
	...zPagingScrollProps,
	...zPagingBackToTopProps,
	...zPagingVirtualListProps,
	/** 初始页码 */
	defaultPageNo: {
		type: Number,
		default: 1,
	},
	/** 每页条数，必须大于 0，并与实际接口的分页条数一致 */
	defaultPageSize: {
		type: Number,
		default: 15,
		validator: (value: number): boolean => value > 0,
	},
	/**
	 * 使用内置滚动容器时是否启用 `fixed` 布局
	 * 开启时组件默认铺满可用高度，父容器无需设置固定高度。
	 */
	fixed: {
		type: Boolean,
		default: false,
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
	 * 页面滚动模式下，内容不足一屏时是否由 z-paging 自动撑满视口
	 * 默认关闭。外层布局已为页脚预留高度，自动撑满可能导致空列表也出现滚动。
	 */
	autoFullHeight: {
		type: Boolean,
		default: false,
	},
	/**
	 * 是否使用页面滚动
	 * 开启时无需给 z-paging 指定滚动容器高度。页面需通过 mixin 或组合式 hook 转发滚动、触底等事件。
	 */
	usePageScroll: {
		type: Boolean,
		default: true,
	},
	/** 根容器背景，支持颜色、渐变等 CSS `background` 值，优先使用 `pagingStyle` 中的背景 */
	bgColor: {
		type: String,
		default: "var(--wot-filled-bottom)",
	},
	/** 传入时控制 z-paging 的自动查询，省略时由 FaPaging 在页面显示时刷新 */
	auto: {
		type: Boolean,
		default: undefined,
	},
	/** reload 时是否显示下拉刷新视图 */
	showRefresherWhenReload: {
		type: Boolean,
		default: true,
	},
	/** reload 时是否显示底部加载中视图 */
	showLoadingMoreWhenReload: {
		type: Boolean,
		default: true,
	},
	/** 是否显示最后更新时间 */
	showRefresherUpdateTime: {
		type: Boolean,
		default: true,
	},
	/** 最后更新时间的存储键，列表间需分别记录时使用不同的值 */
	refresherUpdateTimeKey: {
		type: String,
		default: () => randomString(8),
	},
	/** 空态文案，支持语言映射 */
	emptyViewText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: "暂无数据~",
	},
	/** 重新加载按钮文案，支持语言映射 */
	emptyViewReloadText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: "立即重试",
	},
	/** 加载失败文案，支持语言映射 */
	emptyViewErrorText: {
		type: [String, Object] as PropType<I18nText | null>,
		default: "加载失败，请稍后重试~",
	},
	/** 是否在超过滚动阈值时自动显示返回顶部按钮 */
	autoShowBackToTop: {
		type: Boolean,
		default: true,
	},
	/** 虚拟列表启用时是否关闭自动启用的内置列表，显式设置 `useInnerList` 仍可启用 */
	forceCloseInnerList: {
		type: Boolean,
		default: true,
	},
	/** 完整的本地列表，空数组表示空数据 */
	data: {
		type: Array as PropType<TResult[]>,
		default: undefined,
	},
	/** 请求数据的函数 */
	requestApi: {
		type: Function as PropType<(params?: Partial<TInput>) => Promise<PagedResult<TResult> | TResult[]>>,
	},
	/** 分页 */
	pagination: {
		type: Boolean,
		default: true,
	},
	/** 本地数据数组发生变化时是否重新分页 */
	autoRefresh: {
		type: Boolean,
		default: true,
	},
});

const zPagingProps = useProps(props, props, [
	"pagingClass",
	"pagingContentStyle",
	"auto",
	"loadingMoreEnabled",
	"data",
	"requestApi",
	"pagination",
	"autoRefresh",
]);

type ZPagingEmit = ZPagingBaseEmit &
	ZPagingDataEmit &
	ZPagingNvueEmit &
	ZPagingEmptyViewEmit &
	ZPagingRefresherEmit &
	ZPagingLoadMoreEmit &
	ZPagingChatRecordEmit &
	ZPagingScrollEmit &
	ZPagingBackToTopEmit &
	ZPagingVirtualListEmit;

const emit = defineEmits<
	Omit<ZPagingEmit, "query"> & {
		/** 请求分页数据，参数依次为页码、请求条数和触发来源 */
		query: [pageData: TResult[], pageNo: number, pageSize: number, totalRows: number, from: ZPagingEnums.QueryFrom];
	}
>();

const slots = defineSlots<{
	// 主体布局

	/** 当前用于渲染的列表和查询条件 */
	default?: (props: {
		pagedList: TResult[];
		searchParam: Partial<TInput>;
		setParam: (value: Partial<TInput>) => void;
		refresh: (pageIndex?: number) => Promise<void>;
	}) => VNode[];
	/** 顶部区域，用于导航栏、搜索栏等 */
	top?: (props: {
		pagedList: TResult[];
		searchParam: Partial<TInput>;
		setParam: (value: Partial<TInput>) => void;
		refresh: (pageIndex?: number) => Promise<void>;
	}) => VNode[];
	/** 底部区域，用于底部操作栏等 */
	bottom?: (props: {
		pagedList: TResult[];
		searchParam: Partial<TInput>;
		setParam: (value: Partial<TInput>) => void;
		refresh: (pageIndex?: number) => Promise<void>;
	}) => VNode[];
	/** 列表左侧区域 */
	left?: (props: {
		pagedList: TResult[];
		searchParam: Partial<TInput>;
		setParam: (value: Partial<TInput>) => void;
		refresh: (pageIndex?: number) => Promise<void>;
	}) => VNode[];
	/** 列表右侧区域 */
	right?: (props: {
		pagedList: TResult[];
		searchParam: Partial<TInput>;
		setParam: (value: Partial<TInput>) => void;
		refresh: (pageIndex?: number) => Promise<void>;
	}) => VNode[];

	// 下拉刷新

	/** 自定义下拉刷新视图 */
	refresher?: (props: { refresherStatus: ZPagingEnums.RefresherStatus }) => VNode[];
	/** 刷新完成时的视图，可通过 refresherCompleteDelay 延长展示时间 */
	refresherComplete?: () => VNode[];
	/** 提示松手进入二楼的下拉视图 */
	refresherF2?: () => VNode[];
	/** 进入二楼后显示的内容 */
	f2?: () => VNode[];

	// 底部加载更多

	/** 等待加载更多时的视图 */
	loadingMoreDefault?: () => VNode[];
	/** 正在加载更多时的视图 */
	loadingMoreLoading?: () => VNode[];
	/** 没有更多数据时的视图 */
	loadingMoreNoMore?: () => VNode[];
	/** 加载更多失败时的视图 */
	loadingMoreFail?: () => VNode[];

	// 空态、加载状态与返回顶部

	/** 空数据或请求失败时的视图，由 isLoadFailed 区分 */
	empty?: (props: { isLoadFailed: boolean }) => VNode[];
	/** 列表重载时的加载视图，默认仅在首次加载时显示 */
	loading?: () => VNode[];
	/** 自定义返回顶部按钮 */
	backToTop?: () => VNode[];

	// 虚拟列表与内置列表

	/** 列表项内容，启用 useVirtualList 或 useInnerList 时使用 */
	cell?: (props: { item: TResult; index: number }) => VNode[];
	/** 随列表滚动的头部内容 */
	header?: () => VNode[];
	/** 随列表滚动的尾部内容 */
	footer?: () => VNode[];

	// 聊天记录模式

	/** 聊天列表顶部加载历史记录的视图 */
	chatLoading?: (props: { loadingMoreStatus: ZPagingEnums.LoadMoreStatus }) => VNode[];
	/** 聊天历史记录已全部加载时的视图 */
	chatNoMore?: () => VNode[];
}>();

/** `v-model` 绑定的已加载列表 */
const modelValue = defineModel<TResult[]>({ default: () => [] });
/** 分页搜索参数 */
const searchParam = defineModel<Partial<TInput>>("searchParam", { default: () => ({}) });

const paging = shallowRef<ZPagingRef<TResult> & Pick<ComponentPublicInstance, "$forceUpdate"> & { pageReachBottom: () => void }>();

/** 虚拟列表数据 */
const virtualList = shallowRef<TResult[]>([]);
/** 实际渲染的数据 */
const pagedList = computed(() => (props.useVirtualList && !props.usePageScroll ? virtualList.value : modelValue.value));

/** 查询接口并回填列表、总条数与加载状态。 */
const handleQuery = async (pageNo: number, pageSize: number, from: ZPagingEnums.QueryFrom) => {
	try {
		if (props.requestApi) {
			const apiRes = await props.requestApi({ ...searchParam.value, ...(props.pagination ? { pageIndex: pageNo, pageSize } : {}) });

			let pageData = [] as TResult[];
			let totalRows = 0;
			if (props.pagination) {
				if (Array.isArray(apiRes)) {
					pageData = apiRes ?? [];
					totalRows = pageData.length;
				} else {
					pageData = apiRes.rows ?? [];
					totalRows = apiRes.totalRows ?? pageData.length;
				}
				await paging.value.completeByTotal(pageData, totalRows);
			} else {
				pageData = apiRes as TResult[];
				totalRows = pageData.length;
				await paging.value.setLocalPaging(pageData);
			}
			emit("query", pageData, pageNo, pageSize, totalRows, from);
		} else {
			let pageData = [] as TResult[];
			let totalRows = 0;
			if (props.pagination) {
				const allData = props.data?.slice() ?? [];
				await paging.value.setLocalPaging(allData);

				const size = Math.max(1, pageSize);
				const start = (Math.max(1, pageNo) - 1) * size;
				pageData = allData.slice(start, start + size);
				totalRows = allData.length;
			} else {
				pageData = props.data?.slice() ?? [];
				totalRows = pageData.length;
				await paging.value.completeByNoMore(pageData, true);
			}

			emit("query", pageData, pageNo, pageSize, totalRows, from);
		}
	} catch {
		await paging.value.complete(false);
	}
};

/** 同步虚拟列表需要渲染的数据。 */
const handleVirtualListChange = (list: TResult[]) => {
	virtualList.value = list;
	emit("virtualListChange", list);
};

/** 重新加载列表；传入页码时刷新至该页，省略时从首页加载。 */
const handleRefresh = async (pageIndex?: number): Promise<void> => {
	if (pageIndex) {
		await paging.value.refreshToPage(pageIndex);
	} else {
		// 带动画的 reload 可能在没有视图更新时卡在 z-paging 的 $nextTick 中。
		paging.value.$forceUpdate();
		await paging.value.reload();
	}
};

const handleSearchParam = (value: Partial<TInput>) => {
	searchParam.value = value;
};

watch(
	() => props.data?.slice(),
	() => {
		if (!props.requestApi && props.autoRefresh) {
			handleRefresh();
		}
	},
	{ flush: "post" }
);

// 启用页面滚动时，通过传入的引用转发页面生命周期。
if (props.usePageScroll) {
	useZPaging(paging);
}

onShow(() => {
	// 未传入自动刷新时，页面显示自动调用刷新
	if (props.auto === undefined) {
		nextTick(() => {
			paging.value.reload();
		});
	}
});

defineExpose({
	// 数据刷新与处理

	/**
	 * 从默认页码重新加载分页数据。
	 *
	 * @param animate - 是否播放下拉刷新动画，省略时使用 `show-refresher-when-reload` 的值
	 * @returns 本次重载完成后的总列表与是否已无更多数据
	 */
	reload: computed(() => paging.value?.reload),
	/**
	 * 重新请求已加载页的全部数据，保留当前分页进度。
	 *
	 * @returns 刷新后的总列表与是否已无更多数据
	 */
	refresh: computed(() => paging.value?.refresh),
	/**
	 * 重新加载从首页到指定页的数据。
	 *
	 * @param page - 目标页数
	 * @returns 刷新后的总列表与是否已无更多数据
	 */
	refreshToPage: computed(() => paging.value?.refreshToPage),
	/**
	 * 回填查询结果，按返回条数判断是否还有更多数据。
	 *
	 * @param data - 本次查询结果，传 `false` 表示请求失败
	 * @param success - 是否请求成功，默认 `true`
	 */
	complete: computed(() => paging.value?.complete),
	/**
	 * 回填查询结果，按数据总数判断是否还有更多数据。
	 *
	 * @param data - 本次查询结果
	 * @param total - 数据总数
	 * @param success - 是否请求成功，默认 `true`
	 */
	completeByTotal: computed(() => paging.value?.completeByTotal),
	/**
	 * 回填查询结果，由调用方指定是否还有更多数据。
	 *
	 * @param data - 本次查询结果
	 * @param noMore - 是否已无更多数据
	 * @param success - 是否请求成功，默认 `true`
	 */
	completeByNoMore: computed(() => paging.value?.completeByNoMore),
	/**
	 * 结束本次查询并展示失败原因。
	 *
	 * @param cause - 请求失败原因
	 */
	completeByError: computed(() => paging.value?.completeByError),
	/**
	 * 按 `data-key` 校验并回填结果，避免将过期请求的结果写入当前列表。
	 *
	 * @param data - 本次查询结果
	 * @param key - 本次请求对应的 `data-key`
	 * @param success - 是否请求成功，默认 `true`
	 */
	completeByKey: computed(() => paging.value?.completeByKey),
	/** 清空分页数据并将页码恢复为默认值。 */
	clear: computed(() => paging.value?.clear),
	/**
	 * 从列表顶部插入数据，保留当前页码和每页条数。
	 *
	 * @param data - 单条数据或数据数组
	 * @param scrollToTop - 是否滚动到顶部，默认 `true`
	 * @param animate - 是否使用滚动动画，默认 `true`
	 */
	addDataFromTop: computed(() => paging.value?.addDataFromTop),
	/**
	 * 替换列表数据，保留分页进度且不触发查询。上游建议优先更新 `v-model`。
	 *
	 * @param data - 替换后的列表
	 */
	resetTotalData: computed(() => paging.value?.resetTotalData),

	// 下拉刷新

	/** 手动结束下拉刷新状态。 */
	endRefresh: computed(() => paging.value?.endRefresh),
	/** 重新测量自定义 `refresher` 插槽的高度。 */
	updateCustomRefresherHeight: computed(() => paging.value?.updateCustomRefresherHeight),
	/** 手动进入下拉二楼。 */
	goF2: computed(() => paging.value?.goF2),
	/** 手动关闭下拉二楼。 */
	closeF2: computed(() => paging.value?.closeF2),

	// 底部加载更多

	/**
	 * 手动触发加载更多。
	 *
	 * @param source - 触发来源，可传 `"click"` 或 `"toBottom"`
	 */
	doLoadMore: computed(() => paging.value?.doLoadMore),
	/** 页面滚动模式下，在 `onReachBottom` 中通知 z-paging 处理触底。 */
	pageReachBottom: computed(() => paging.value?.pageReachBottom),

	// 页面滚动与布局

	/**
	 * 页面滚动模式下，同步页面的纵向滚动位置。已接入官方 hook 或 mixin 时由其转发。
	 *
	 * @param scrollTop - 页面 `onPageScroll` 提供的滚动位置
	 */
	updatePageScrollTop: computed(() => paging.value?.updatePageScrollTop),
	/** 页面滚动模式下，重新测量 `top` 插槽高度。 */
	updatePageScrollTopHeight: computed(() => paging.value?.updatePageScrollTopHeight),
	/** 页面滚动模式下，重新测量 `bottom` 插槽高度。 */
	updatePageScrollBottomHeight: computed(() => paging.value?.updatePageScrollBottomHeight),
	/** 左右插槽宽度变化后，重新计算布局。 */
	updateLeftAndRightWidth: computed(() => paging.value?.updateLeftAndRightWidth),
	/** 更新 `fixed` 模式的布局，可在页面返回时修正底部安全区占位。 */
	updateFixedLayout: computed(() => paging.value?.updateFixedLayout),

	// 虚拟列表

	/**
	 * 在动态高度虚拟列表的指定位置插入数据。
	 *
	 * @param item - 要插入的数据项
	 * @param index - 插入位置，从 `0` 开始
	 */
	doInsertVirtualListItem: computed(() => paging.value?.doInsertVirtualListItem),
	/**
	 * 虚拟列表项高度变化后，更新该项的高度缓存。
	 *
	 * @param index - 列表项索引，从 `0` 开始
	 */
	didUpdateVirtualListCell: computed(() => paging.value?.didUpdateVirtualListCell),
	/**
	 * 删除虚拟列表项后，同步移除该项的高度缓存。
	 *
	 * @param index - 被删除项的原索引，从 `0` 开始
	 */
	didDeleteVirtualListCell: computed(() => paging.value?.didDeleteVirtualListCell),
	/** 手动重新计算虚拟列表的渲染范围。 */
	updateVirtualListRender: computed(() => paging.value?.updateVirtualListRender),

	// 本地分页

	/**
	 * 将完整数据交给 z-paging 做本地分页，加载更多时由组件内部切分数据。
	 *
	 * @param data - 用于本地分页的完整列表
	 * @param success - 是否请求成功，默认 `true`
	 */
	setLocalPaging: computed(() => paging.value?.setLocalPaging),

	// 聊天记录

	/** 聊天记录模式下，手动触发顶部加载更多。 */
	doChatRecordLoadMore: computed(() => paging.value?.doChatRecordLoadMore),
	/**
	 * 聊天记录模式下，添加一条或多条消息。
	 *
	 * @param data - 单条消息或消息数组
	 * @param scrollToBottom - 是否滚动到底部，默认 `true`
	 * @param animate - 是否使用滚动动画，默认 `true`
	 */
	addChatRecordData: computed(() => paging.value?.addChatRecordData),
	/** 手动注册键盘高度变化监听，可用于返回聊天页时恢复监听。 */
	addKeyboardHeightChangeListener: computed(() => paging.value?.addKeyboardHeightChangeListener),

	// 滚动到指定位置

	/**
	 * 滚动到列表顶部。
	 *
	 * @param animate - 是否使用动画，默认 `true`
	 */
	scrollToTop: computed(() => paging.value?.scrollToTop),
	/**
	 * 滚动到列表底部。
	 *
	 * @param animate - 是否使用动画，默认 `true`
	 */
	scrollToBottom: computed(() => paging.value?.scrollToBottom),
	/**
	 * 在 Vue 渲染模式下，按元素 ID 滚动到指定位置。
	 *
	 * @param id - 目标元素 ID，无需 `#` 前缀
	 * @param offset - 偏移量，单位 `px`，默认 `0`
	 * @param animate - 是否使用动画，默认 `false`
	 */
	scrollIntoViewById: computed(() => paging.value?.scrollIntoViewById),
	/**
	 * 在 Vue 渲染模式下，按节点测量得到的 `top` 值滚动。
	 *
	 * @param top - 通过 `uni.createSelectorQuery()` 测量得到的节点 `top`
	 * @param offset - 偏移量，单位 `px`，默认 `0`
	 * @param animate - 是否使用动画，默认 `false`
	 */
	scrollIntoViewByNodeTop: computed(() => paging.value?.scrollIntoViewByNodeTop),
	/**
	 * 在 Vue 渲染模式下，滚动到指定纵向位置。
	 *
	 * @param y - 距顶部的距离，单位 `px`
	 * @param offset - 偏移量，单位 `px`，默认 `0`
	 * @param animate - 是否使用动画，默认 `false`
	 */
	scrollToY: computed(() => paging.value?.scrollToY),
	/**
	 * 在 Vue 渲染模式下，使用内置滚动容器横向滚动到指定位置。
	 *
	 * @param x - 距左侧的距离，单位 `px`
	 * @param offset - 偏移量，单位 `px`，默认 `0`
	 * @param animate - 是否使用动画，默认 `false`
	 */
	scrollToX: computed(() => paging.value?.scrollToX),
	/**
	 * 按索引滚动到指定列表项，适用于虚拟列表或 nvue。
	 *
	 * @param index - 目标索引，nvue 的列表项需设置 `ref="z-paging-${index}"`
	 * @param offset - 偏移量，单位 `px`，默认 `0`
	 * @param animate - 是否使用动画，默认 `false`
	 */
	scrollIntoViewByIndex: computed(() => paging.value?.scrollIntoViewByIndex),
	/**
	 * 在 nvue 中滚动到指定节点。
	 *
	 * @param view - 通过组件引用取得的目标节点
	 * @param offset - 偏移量，单位 `px`，默认 `0`
	 * @param animate - 是否使用动画，默认 `false`
	 */
	scrollIntoViewByView: computed(() => paging.value?.scrollIntoViewByView),
	/** 设置 nvue 列表的 `specialEffects`。 */
	setSpecialEffects: computed(() => paging.value?.setSpecialEffects),

	// nvue 专用

	/** `setSpecialEffects` 的别名。 */
	setListSpecialEffects: computed(() => paging.value?.setListSpecialEffects),

	// 缓存

	/** 用当前列表的前 `pageSize` 条数据覆盖缓存。 */
	updateCache: computed(() => paging.value?.updateCache),

	// 版本

	/** 获取 z-paging 的当前版本号。 */
	getVersion: computed(() => paging.value?.getVersion),
});
</script>
