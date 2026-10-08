import { reactive, ref, shallowRef } from "vue";
import useZPaging from "z-paging/components/z-paging/js/hooks/useZPaging.js";
import type { Reactive, Ref, ShallowRef } from "vue";
import type { ZPagingProps } from "z-paging/types/comps/z-paging";

type UsePagingResult<TResult, TInput> = {
	/** ZPaging 组件实例 */
	paging: Ref<ZPagingRef<TResult>>;
	/** 搜索参数 */
	searchParam: Reactive<PagedInput & TInput>;
	/** 分页数据集合 */
	pagedList: ShallowRef<TResult[]>;
	/** 分页查询 */
	pagingQuery: ZPagingProps["onQuery"];
	/** 虚拟列表数据改变回调 */
	pagingVirtualListChange: (list: TResult[]) => void;
	/** 刷新列表；指定页码时刷新到该页，否则从首页重新加载。 */
	pagingRefresh: {
		(): void;
		(pageIndex: number): void;
	};
};

/**
 * 将分页接口接入 z-paging 的查询、刷新与虚拟列表事件。
 *
 * @remarks
 * 调用方需将 paging 绑定到组件 ref；请求失败通过 complete(false) 结束加载并显示失败状态。
 * searchParam 会覆盖同名分页参数，使用时应避免保留过期的 pageIndex 和 pageSize。
 *
 * @param requestApi - 返回分页结果的接口，列表数据取自 rows
 * @param pageScroll - 是否注册 z-paging 的页面滚动生命周期
 * @returns 分页组件引用、查询参数、列表数据和事件处理方法
 */
export const usePaging = <TResult, TInput extends Record<string, unknown> = Record<string, never>>(
	requestApi: (params?: PagedInput & TInput) => Promise<PagedResult<TResult>>,
	pageScroll?: boolean
): UsePagingResult<TResult, TInput> => {
	// ZPaging 组件实例
	const zPagingRef = ref<ZPagingRef<TResult>>();

	// 搜索参数
	const searchParam = reactive<PagedInput & TInput>({} as PagedInput & TInput);

	// 分页数据集合
	const pagedList = shallowRef<TResult[]>([]);

	/** 分页查询。 */
	const pagingQuery: ZPagingProps["onQuery"] = (pageNo, pageSize) => {
		requestApi({
			pageIndex: pageNo,
			pageSize,
			...(searchParam as TInput),
		})
			.then((apiRes) => zPagingRef.value.complete(apiRes.rows))
			.catch(() => zPagingRef.value.complete(false));
	};

	/** 虚拟列表数据改变回调。 */
	const pagingVirtualListChange: ZPagingProps["onVirtualListChange"] = (list: TResult[]) => {
		pagedList.value = list;
	};

	/** 刷新列表；指定页码时刷新到该页，否则从首页重新加载。 */
	function pagingRefresh(): void;
	function pagingRefresh(pageIndex?: number) {
		if (pageIndex) {
			zPagingRef.value?.refreshToPage(pageIndex);
		} else {
			zPagingRef.value?.reload();
		}
	}

	// 页面滚动模式需要将页面生命周期交给 z-paging 处理。
	if (pageScroll) {
		useZPaging(zPagingRef);
	}

	return {
		// z-paging 约定通过 paging 属性获取分页组件实例。
		paging: zPagingRef,
		searchParam,
		pagedList,
		pagingQuery,
		pagingVirtualListChange,
		pagingRefresh,
	};
};
