import { axiosUtil } from "@fast-china/axios";
import type { ElSelectorOutput, PagedResult } from "fast-element-plus";
import type { AddRoleInput } from "./models/AddRoleInput";
import type { EditRoleInput } from "./models/EditRoleInput";
import type { QueryRoleDetailOutput } from "./models/QueryRoleDetailOutput";
import type { QueryRolePagedInput } from "./models/QueryRolePagedInput";
import type { QueryRolePagedOutput } from "./models/QueryRolePagedOutput";
import type { RoleAuthInput } from "./models/RoleAuthInput";
import type { RoleIdInput } from "./models/RoleIdInput";

/**
 * 角色服务Api
 */
export const roleApi = {
	/**
	 * 角色选择器
	 */
	roleSelector(): Promise<ElSelectorOutput<string>[]> {
		return axiosUtil.request<ElSelectorOutput<string>[]>({
			url: "/role/roleSelector",
			method: "get",
			requestType: "query",
		});
	},
	/**
	 * 获取角色分页列表
	 */
	queryRolePaged(data: QueryRolePagedInput): Promise<PagedResult<QueryRolePagedOutput>> {
		return axiosUtil.request<PagedResult<QueryRolePagedOutput>>({
			url: "/role/queryRolePaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取角色详情
	 */
	queryRoleDetail(roleId: string): Promise<QueryRoleDetailOutput> {
		return axiosUtil.request<QueryRoleDetailOutput>({
			url: "/role/queryRoleDetail",
			method: "get",
			params: {
				roleId,
			},
			requestType: "query",
		});
	},
	/**
	 * 添加角色
	 */
	addRole(data: AddRoleInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/role/addRole",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑角色
	 */
	editRole(data: EditRoleInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/role/editRole",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 删除角色
	 */
	deleteRole(data: RoleIdInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/role/deleteRole",
			method: "post",
			data,
			requestType: "delete",
		});
	},
	/**
	 * 角色授权
	 */
	roleAuth(data: RoleAuthInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/role/roleAuth",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 获取角色授权菜单
	 */
	queryRoleAuthMenu(data: RoleIdInput): Promise<RoleAuthInput> {
		return axiosUtil.request<RoleAuthInput>({
			url: "/role/queryRoleAuthMenu",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取授权菜单
	 */
	queryAuthMenu(): Promise<ElSelectorOutput<string>[]> {
		return axiosUtil.request<ElSelectorOutput<string>[]>({
			url: "/role/queryAuthMenu",
			method: "get",
			requestType: "query",
		});
	},
};
