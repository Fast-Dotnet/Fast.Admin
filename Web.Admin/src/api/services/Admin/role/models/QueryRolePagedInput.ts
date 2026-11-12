import type { PagedInput } from "fast-element-plus";
import type { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import type { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";

/**
 * 获取角色分页列表输入
 */
export interface QueryRolePagedInput extends PagedInput  {
	/**
	 * 
	 */
	roleType?: RoleTypeEnum;
	/**
	 * 
	 */
	dataScopeType?: DataScopeTypeEnum;
}

