import { defineStore } from "pinia";

/** 分页版本 */
export const usePaging = defineStore("paging", () => {
	/** 页面版本号集合，仅在当前运行期间有效 */
	const pageVersions = new Map<string, number>();

	/**
	 * 获取页面版本号
	 * @param page - 页面路径，默认当前页面
	 */
	const getPageVersion = (page?: string): number => {
		const pageKey = (page ?? getCurrentPages().at(-1)?.route)?.replace(/^\/+/, "");
		if (!pageKey) return 0;

		return pageVersions.get(pageKey) ?? 0;
	};

	/**
	 * 递增页面版本号
	 * @param page - 页面路径，默认当前页面
	 */
	const incrementPageVersion = (page?: string): number => {
		const pageKey = (page ?? getCurrentPages().at(-1)?.route)?.replace(/^\/+/, "");
		if (!pageKey) return 0;

		const version = getPageVersion(pageKey) + 1;
		pageVersions.set(pageKey, version);
		return version;
	};

	return {
		getPageVersion,
		incrementPageVersion,
	};
});
