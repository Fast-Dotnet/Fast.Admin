import type { PagedInput } from "fast-element-plus";
import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";

/**
 * 获取在线用户分页列表输入
 */
export interface QueryTenantOnlineUserPagedInput extends PagedInput  {
	/**
	 * 
	 */
	deviceType?: AppEnvironmentEnum;
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 职员Id
	 */
	employeeId?: string;
}

