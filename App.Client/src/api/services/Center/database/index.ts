import { axiosUtil } from "@fast-china/axios";
import type { AddDatabaseInput } from "./models/AddDatabaseInput";
import type { EditDatabaseInput } from "./models/EditDatabaseInput";
import type { InitDatabaseInput } from "./models/InitDatabaseInput";
import type { MainIdInput } from "./models/MainIdInput";
import type { QueryDatabaseDetailOutput } from "./models/QueryDatabaseDetailOutput";
import type { QueryDatabasePagedInput } from "./models/QueryDatabasePagedInput";
import type { QueryDatabasePagedOutput } from "./models/QueryDatabasePagedOutput";

/**
 * 数据库服务Api
 */
export const databaseApi = {
	/**
	 * 获取数据库分页列表
	 */
	queryDatabasePaged(data: QueryDatabasePagedInput): Promise<PagedResult<QueryDatabasePagedOutput>> {
		return axiosUtil.request<PagedResult<QueryDatabasePagedOutput>>({
			url: "/database/queryDatabasePaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取数据库详情
	 */
	queryDatabaseDetail(mainId: string): Promise<QueryDatabaseDetailOutput> {
		return axiosUtil.request<QueryDatabaseDetailOutput>({
			url: "/database/queryDatabaseDetail",
			method: "get",
			params: {
				mainId,
			},
			requestType: "query",
		});
	},
	/**
	 * 添加数据库
	 */
	addDatabase(data: AddDatabaseInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/database/addDatabase",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑数据库
	 */
	editDatabase(data: EditDatabaseInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/database/editDatabase",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 删除数据库
	 */
	deleteDatabase(data: MainIdInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/database/deleteDatabase",
			method: "post",
			data,
			requestType: "delete",
		});
	},
	/**
	 * 初始化数据库
	 */
	initDatabase(data: InitDatabaseInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/database/initDatabase",
			method: "post",
			data,
			requestType: "submit",
		});
	},
};
