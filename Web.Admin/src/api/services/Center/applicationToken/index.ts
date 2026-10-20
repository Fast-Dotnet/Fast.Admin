import { axiosUtil } from "@fast-china/axios";
import type { PagedInput, PagedResult } from "fast-element-plus";
import type { AddApplicationTokenInput } from "./models/AddApplicationTokenInput";
import type { DeleteApplicationTokenInput } from "./models/DeleteApplicationTokenInput";
import type { QueryApplicationTokenPagedOutput } from "./models/QueryApplicationTokenPagedOutput";

/**
 * 应用凭据服务Api
 */
export const applicationTokenApi = {
	/**
	 * 获取应用凭据分页列表
	 */
	queryApplicationTokenPaged(data: PagedInput): Promise<PagedResult<QueryApplicationTokenPagedOutput>> {
		return axiosUtil.request<PagedResult<QueryApplicationTokenPagedOutput>>({
			url: "/applicationToken/queryApplicationTokenPaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 添加应用凭据
	 */
	addApplicationToken(data: AddApplicationTokenInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationToken/addApplicationToken",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 删除应用凭据
	 */
	deleteApplicationToken(data: DeleteApplicationTokenInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationToken/deleteApplicationToken",
			method: "post",
			data,
			requestType: "delete",
		});
	},
	/**
	 * 刷新应用凭据
	 */
	refreshApplicationToken(data: DeleteApplicationTokenInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationToken/refreshApplicationToken",
			method: "post",
			data,
			requestType: "edit",
		});
	},
};
