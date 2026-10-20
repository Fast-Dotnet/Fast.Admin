import { axiosUtil } from "@fast-china/axios";
import type { AddApplicationClientInput } from "./models/AddApplicationClientInput";
import type { DeleteApplicationClientInput } from "./models/DeleteApplicationClientInput";
import type { EditApplicationClientInput } from "./models/EditApplicationClientInput";
import type { QueryApplicationClientDetailOutput } from "./models/QueryApplicationClientDetailOutput";

/**
 * 应用标识服务Api
 */
export const applicationClientApi = {
	/**
	 * 获取应用标识详情
	 */
	queryApplicationClientDetail(recordId: string): Promise<QueryApplicationClientDetailOutput> {
		return axiosUtil.request<QueryApplicationClientDetailOutput>({
			url: "/applicationClient/queryApplicationClientDetail",
			method: "get",
			params: {
				recordId,
			},
			requestType: "query",
		});
	},
	/**
	 * 添加应用标识
	 */
	addApplicationClient(data: AddApplicationClientInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationClient/addApplicationClient",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑应用标识
	 */
	editApplicationClient(data: EditApplicationClientInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationClient/editApplicationClient",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 删除应用标识
	 */
	deleteApplicationClient(data: DeleteApplicationClientInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/applicationClient/deleteApplicationClient",
			method: "post",
			data,
			requestType: "delete",
		});
	},
};
