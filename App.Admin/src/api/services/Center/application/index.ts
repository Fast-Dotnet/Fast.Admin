import { axiosUtil } from "@fast-china/axios";
import type { AddApplicationInput } from "./models/AddApplicationInput";
import type { AppIdInput } from "./models/AppIdInput";
import type { EditApplicationInput } from "./models/EditApplicationInput";
import type { QueryAppDownloadInput } from "./models/QueryAppDownloadInput";
import type { QueryAppDownloadOutput } from "./models/QueryAppDownloadOutput";
import type { QueryApplicationDetailOutput } from "./models/QueryApplicationDetailOutput";
import type { QueryApplicationPagedInput } from "./models/QueryApplicationPagedInput";
import type { QueryApplicationPagedOutput } from "./models/QueryApplicationPagedOutput";

/**
 * 应用服务Api
 */
export const applicationApi = {
	/**
	 * 应用选择器
	 */
	applicationSelector(): Promise<ElSelectorOutput<string>[]> {
		return axiosUtil.request<ElSelectorOutput<string>[]>({
			url: "/application/applicationSelector",
			method: "get",
			requestType: "query",
		});
	},
	/**
	 * 获取应用分页列表
	 */
	queryApplicationPaged(data: QueryApplicationPagedInput): Promise<PagedResult<QueryApplicationPagedOutput>> {
		return axiosUtil.request<PagedResult<QueryApplicationPagedOutput>>({
			url: "/application/queryApplicationPaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取应用详情
	 */
	queryApplicationDetail(appId: string): Promise<QueryApplicationDetailOutput> {
		return axiosUtil.request<QueryApplicationDetailOutput>({
			url: "/application/queryApplicationDetail",
			method: "get",
			params: {
				appId,
			},
			requestType: "query",
		});
	},
	/**
	 * 添加应用
	 */
	addApplication(data: AddApplicationInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/application/addApplication",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑应用
	 */
	editApplication(data: EditApplicationInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/application/editApplication",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 删除应用
	 */
	deleteApplication(data: AppIdInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/application/deleteApplication",
			method: "post",
			data,
			requestType: "delete",
		});
	},
	/**
	 * 获取应用下载
	 */
	queryAppDownload(data: QueryAppDownloadInput): Promise<QueryAppDownloadOutput> {
		return axiosUtil.request<QueryAppDownloadOutput>({
			url: "/application/queryAppDownload",
			method: "post",
			data,
			requestType: "query",
		});
	},
};
