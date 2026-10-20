import type { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import type { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";

/**
 * 获取角色分页列表输出
 */
export interface QueryRolePagedOutput {
	/**
	 * 角色Id
	 */
	roleId?: string;
	/**
	 * 
	 */
	roleType?: RoleTypeEnum;
	/**
	 * 是否使用系统菜单
	 */
	isSystemMenu?: boolean;
	/**
	 * 角色名称
	 */
	roleName?: string;
	/**
	 * 角色编码
	 */
	roleCode?: string;
	/**
	 * 排序
	 */
	sort?: number;
	/**
	 * 
	 */
	dataScopeType?: DataScopeTypeEnum;
	/**
	 * 备注
	 */
	remark?: string;
	/**
	 * 
	 */
	createdUserName?: string;
	/**
	 * 
	 */
	createdTime?: string;
	/**
	 * 
	 */
	updatedUserName?: string;
	/**
	 * 
	 */
	updatedTime?: string;
	/**
	 * 
	 */
	rowVersion?: string;
}

