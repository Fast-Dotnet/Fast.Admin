import { axiosUtil } from "@fast-china/axios";
import type { AddTableConfigInput } from "./models/AddTableConfigInput";
import type { CopyTableConfigInput } from "./models/CopyTableConfigInput";
import type { EditTableColumnConfigInput } from "./models/EditTableColumnConfigInput";
import type { EditTableConfigInput } from "./models/EditTableConfigInput";
import type { FaTableColumnCtx } from "./models/FaTableColumnCtx";
import type { QueryTableColumnConfigOutput } from "./models/QueryTableColumnConfigOutput";
import type { QueryTableConfigDetailOutput } from "./models/QueryTableConfigDetailOutput";
import type { QueryTableConfigPagedOutput } from "./models/QueryTableConfigPagedOutput";
import type { SaveUserTableConfigInput } from "./models/SaveUserTableConfigInput";
import type { SyncUserTableConfigInput } from "./models/SyncUserTableConfigInput";
import type { TableIdInput } from "./models/TableIdInput";

/**
 * 表格服务Api
 */
export const tableApi = {
	/**
	 * 获取表格配置分页列表
	 */
	queryTableConfigPaged(data: PagedInput): Promise<PagedResult<QueryTableConfigPagedOutput>> {
		return axiosUtil.request<PagedResult<QueryTableConfigPagedOutput>>({
			url: "/table/queryTableConfigPaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取表格配置详情
	 */
	queryTableConfigDetail(tableId: string): Promise<QueryTableConfigDetailOutput> {
		return axiosUtil.request<QueryTableConfigDetailOutput>({
			url: "/table/queryTableConfigDetail",
			method: "get",
			params: {
				tableId,
			},
			requestType: "query",
		});
	},
	/**
	 * 添加表格配置
	 */
	addTableConfig(data: AddTableConfigInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/addTableConfig",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑表格配置
	 */
	editTableConfig(data: EditTableConfigInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/editTableConfig",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 删除表格配置
	 */
	deleteTableConfig(data: TableIdInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/deleteTableConfig",
			method: "post",
			data,
			requestType: "delete",
		});
	},
	/**
	 * 复制表格配置
	 */
	copyTableConfig(data: CopyTableConfigInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/copyTableConfig",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 获取表格列配置详情
	 */
	queryTableColumnConfigDetail(tableId: string): Promise<FaTableColumnCtx[]> {
		return axiosUtil.request<FaTableColumnCtx[]>({
			url: "/table/queryTableColumnConfigDetail",
			method: "get",
			params: {
				tableId,
			},
			requestType: "query",
		});
	},
	/**
	 * 编辑表格列配置
	 */
	editTableColumnConfig(data: EditTableColumnConfigInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/editTableColumnConfig",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 获取表格列配置
	 */
	queryTableColumnConfig(tableKey: string): Promise<QueryTableColumnConfigOutput> {
		return axiosUtil.request<QueryTableColumnConfigOutput>({
			url: "/table/queryTableColumnConfig",
			method: "get",
			params: {
				tableKey,
			},
			requestType: "query",
		});
	},
	/**
	 * 同步用户表格配置
	 */
	syncUserTableConfig(data: SyncUserTableConfigInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/syncUserTableConfig",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 保存用户表格配置
	 */
	saveUserTableConfig(data: SaveUserTableConfigInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/saveUserTableConfig",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 清除用户表格配置
	 */
	clearUserTableConfig(data: SyncUserTableConfigInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/table/clearUserTableConfig",
			method: "post",
			data,
			requestType: "delete",
		});
	},
};
