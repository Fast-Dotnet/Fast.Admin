import { axiosUtil } from "@fast-china/axios";

/**
 * 地区服务Api
 */
export const regionApi = {
	/**
	 * 地区选择器
	 */
	regionSelector(): Promise<ElSelectorOutput<string>[]> {
		return axiosUtil.request<ElSelectorOutput<string>[]>({
			url: "/region/regionSelector",
			method: "get",
			requestType: "query",
      cache: true,
		});
	},
	/**
	 * 省份选择器
	 */
	provinceSelector(): Promise<ElSelectorOutput<string>[]> {
		return axiosUtil.request<ElSelectorOutput<string>[]>({
			url: "/region/provinceSelector",
			method: "get",
			requestType: "query",
      cache: true,
		});
	},
	/**
	 * 城市选择器
	 */
	citySelector(): Promise<ElSelectorOutput<string>[]> {
		return axiosUtil.request<ElSelectorOutput<string>[]>({
			url: "/region/citySelector",
			method: "get",
			requestType: "query",
      cache: true,
		});
	},
};
