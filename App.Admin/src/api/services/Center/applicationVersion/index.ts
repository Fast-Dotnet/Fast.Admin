import { axiosUtil } from "@fast-china/axios";
import type { AddApplicationVersionInput } from "./models/AddApplicationVersionInput";
import type { EditApplicationVersionInput } from "./models/EditApplicationVersionInput";
import type { QueryApplicationVersionDetailOutput } from "./models/QueryApplicationVersionDetailOutput";
import type { QueryApplicationVersionPagedInput } from "./models/QueryApplicationVersionPagedInput";
import type { QueryApplicationVersionPagedOutput } from "./models/QueryApplicationVersionPagedOutput";

/**
 * 应用版本服务Api
 */
export const applicationVersionApi = {
	/**
	 * 获取应用版本分页列表
	 */
	queryApplicationVersionPaged(data: QueryApplicationVersionPagedInput): Promise<PagedResult<QueryApplicationVersionPagedOutput>> {
		return axiosUtil.request<PagedResult<QueryApplicationVersionPagedOutput>>({
			url: "/applicationVersion/queryApplicationVersionPaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取应用版本详情
	 */
	queryApplicationVersionDetail(recordId: string): Promise<QueryApplicationVersionDetailOutput> {
		return axiosUtil.request<QueryApplicationVersionDetailOutput>({
			url: "/applicationVersion/queryApplicationVersionDetail",
			method: "get",
			params: {
				recordId,
			},
			requestType: "query",
		});
	},
	/**
	 * 添加应用版本
	 */
	addApplicationVersion(data: AddApplicationVersionInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationVersion/addApplicationVersion",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑应用版本
	 */
	editApplicationVersion(data: EditApplicationVersionInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationVersion/editApplicationVersion",
			method: "post",
			data,
			requestType: "edit",
		});
	},
};
