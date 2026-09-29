import { axiosUtil } from "@fast-china/axios";
import type { ElSelectorOutput, PagedInput, PagedResult } from "fast-element-plus";
import type { EditClientUserInput } from "./models/EditClientUserInput";
import type { QueryClientUserDetailOutput } from "./models/QueryClientUserDetailOutput";
import type { QueryClientUserPagedInput } from "./models/QueryClientUserPagedInput";
import type { QueryClientUserPagedOutput } from "./models/QueryClientUserPagedOutput";

/**
 * 客户端用户服务Api
 */
export const clientUserApi = {
	/**
	 * 客户端用户选择器
	 */
	clientUserSelector(data: PagedInput): Promise<PagedResult<ElSelectorOutput<string>>> {
		return axiosUtil.request<PagedResult<ElSelectorOutput<string>>>({
			url: "/clientUser/clientUserSelector",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取客户端用户分页列表
	 */
	queryClientUserPaged(data: QueryClientUserPagedInput): Promise<PagedResult<QueryClientUserPagedOutput>> {
		return axiosUtil.request<PagedResult<QueryClientUserPagedOutput>>({
			url: "/clientUser/queryClientUserPaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取客户端用户详情
	 */
	queryClientUserDetail(): Promise<QueryClientUserDetailOutput> {
		return axiosUtil.request<QueryClientUserDetailOutput>({
			url: "/clientUser/queryClientUserDetail",
			method: "get",
			requestType: "query",
		});
	},
	/**
	 * 编辑客户端用户
	 */
	editClientUser(data: EditClientUserInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/clientUser/editClientUser",
			method: "post",
			data,
			requestType: "edit",
		});
	},
};
