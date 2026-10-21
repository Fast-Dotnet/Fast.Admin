import { axiosUtil } from "@fast-china/axios";
import type { AddMessageTemplateInput } from "./models/AddMessageTemplateInput";
import type { DeleteMessageTemplateInput } from "./models/DeleteMessageTemplateInput";
import type { EditMessageTemplateInput } from "./models/EditMessageTemplateInput";
import type { QueryMessageTemplateDetailOutput } from "./models/QueryMessageTemplateDetailOutput";
import type { QueryMessageTemplatePagedInput } from "./models/QueryMessageTemplatePagedInput";
import type { QueryMessageTemplatePagedOutput } from "./models/QueryMessageTemplatePagedOutput";

/**
 * 消息模板服务Api
 */
export const messageTemplateApi = {
	/**
	 * 获取消息模板分页列表
	 */
	queryMessageTemplatePaged(data: QueryMessageTemplatePagedInput): Promise<PagedResult<QueryMessageTemplatePagedOutput>> {
		return axiosUtil.request<PagedResult<QueryMessageTemplatePagedOutput>>({
			url: "/messageTemplate/queryMessageTemplatePaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取消息模板详情
	 */
	queryMessageTemplateDetail(recordId: string): Promise<QueryMessageTemplateDetailOutput> {
		return axiosUtil.request<QueryMessageTemplateDetailOutput>({
			url: "/messageTemplate/queryMessageTemplateDetail",
			method: "get",
			params: {
				recordId,
			},
			requestType: "query",
		});
	},
	/**
	 * 添加消息模板
	 */
	addMessageTemplate(data: AddMessageTemplateInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/messageTemplate/addMessageTemplate",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑消息模板
	 */
	editMessageTemplate(data: EditMessageTemplateInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/messageTemplate/editMessageTemplate",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 删除消息模板
	 */
	deleteMessageTemplate(data: DeleteMessageTemplateInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/messageTemplate/deleteMessageTemplate",
			method: "post",
			data,
			requestType: "delete",
		});
	},
};
