import { reactive, ref, shallowRef } from "vue";
import useZPaging from "z-paging/components/z-paging/js/hooks/useZPaging.js";
import type { Reactive, Ref, ShallowRef } from "vue";

type UsePagingResult<TResult, TInput> = {
	/**
	 * z-paging 组件引用，需绑定到组件 ref
	 *
	 * @remarks
	 * 为兼容 z-paging mixin，调用方统一保留 paging 命名并绑定 ref="paging"。
	 * 该 mixin 内部通过 this.$refs.paging 获取组件实例。
	 */
	paging: Ref<ZPagingRef<TResult>>;
	/** 响应式查询条件，查询时会附加当前页码和每页条数 */
	searchParam: Reactive<TInput>;
	/** 虚拟列表待渲染数据，由 pagingVirtualListChange 更新 */
	pagedList: ShallowRef<TResult[]>;
	/** z-paging 的 query 事件回调 */
	pagingQuery: (pageNo: number, pageSize: number, from: ZPagingEnums.QueryFrom) => Promise<void>;
	/** z-paging 的 virtualListChange 事件回调 */
	pagingVirtualListChange: (list: TResult[]) => void;
	/** 重新加载列表；传入页码时刷新至该页，省略时从首页加载。 */
	pagingRefresh: {
		(): Promise<void>;
		(pageIndex: number): Promise<void>;
	};
};

/**
 * 封装 z-paging 的接口查询、总条数回填、列表刷新和虚拟列表数据同步。
 *
 * @remarks
 * 将 paging 绑定到组件 ref，并将 pagingQuery、pagingVirtualListChange 绑定到对应事件。
 * 默认配合 z-paging mixin 使用，调用方需保留 paging 命名，并绑定 ref="paging"。
 * 请求中的 pageIndex 和 pageSize 以 z-paging 当前分页事件为准，覆盖 searchParam 中的同名字段。
 * 查询结果通过 completeByTotal 回填；失败时通过 complete(false) 结束加载并显示失败状态。
 *
 * @param requestApi - 查询接口，支持分页结果对象或数据数组
 * @param pageScroll - 是否转发页面滚动、触底和下拉刷新事件，默认 true；启用时需配合 use-page-scroll
 * @param pagination - 是否按分页结果解析响应，默认 true；为 false 时接口必须返回数据数组
 * @returns 组件引用、响应式查询条件、虚拟列表数据及查询和刷新回调
 */
export const usePaging = <TResult, TInput extends PagedInput = PagedInput>(
	requestApi: (params?: TInput) => Promise<PagedResult<TResult> | TResult[]>,
	pageScroll: boolean = true,
	pagination: boolean = true
): UsePagingResult<TResult, TInput> => {
	// z-paging 组件引用
	const zPagingRef = ref<ZPagingRef<TResult>>();

	// 搜索参数
	const searchParam = reactive<TInput>({} as TInput);

	// 虚拟列表待渲染数据
	const pagedList = shallowRef<TResult[]>([]);

	/** 查询接口并回填列表、总条数与加载状态。 */
	const pagingQuery = async (pageNo: number, pageSize: number, _: ZPagingEnums.QueryFrom): Promise<void> => {
		if (!zPagingRef.value) return;

		try {
			// 查询条件由普通对象创建，按请求类型展开，避免泛型 Reactive 的集合分支被误判为 Map。
			const apiRes = await requestApi({ ...(searchParam as TInput), pageIndex: pageNo, pageSize });

			let pageData: TResult[];
			let totalRows: number;
			if (pagination) {
				if (Array.isArray(apiRes)) {
					pageData = apiRes ?? [];
					totalRows = pageData.length;
				} else {
					pageData = apiRes.rows ?? [];
					totalRows = apiRes.totalRows ?? pageData.length;
				}
			} else {
				pageData = apiRes as TResult[];
				totalRows = pageData.length;
			}
			await zPagingRef.value.completeByTotal(pageData, totalRows);
		} catch {
			await zPagingRef.value.complete(false);
		}
	};

	/** 同步虚拟列表需要渲染的数据。 */
	const pagingVirtualListChange = (list: TResult[]) => {
		pagedList.value = list;
	};

	/** 重新加载列表；传入页码时刷新至该页，省略时从首页加载。 */
	async function pagingRefresh(): Promise<void>;
	async function pagingRefresh(pageIndex?: number): Promise<void> {
		if (pageIndex) {
			await zPagingRef.value?.refreshToPage(pageIndex);
		} else {
			await zPagingRef.value?.reload();
		}
	}

	// 启用页面滚动时，通过传入的引用转发页面生命周期。
	if (pageScroll) {
		useZPaging(zPagingRef);
	}

	return {
		paging: zPagingRef,
		searchParam,
		pagedList,
		pagingQuery,
		pagingVirtualListChange,
		pagingRefresh,
	};
};
