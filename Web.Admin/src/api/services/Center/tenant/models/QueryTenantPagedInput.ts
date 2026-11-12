import type { PagedInput } from "fast-element-plus";
import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { EditionEnum } from "@/api/enums/EditionEnum";
import type { TenantTypeEnum } from "@/api/enums/TenantTypeEnum";

/**
 * 获取租户分页列表输入
 */
export interface QueryTenantPagedInput extends PagedInput  {
	/**
	 * 
	 */
	status?: CommonStatusEnum;
	/**
	 * 
	 */
	edition?: EditionEnum;
	/**
	 * 租户管理员手机
	 */
	adminMobile?: string;
	/**
	 * 租户管理员邮箱
	 */
	adminEmail?: string;
	/**
	 * 
	 */
	tenantType?: TenantTypeEnum;
}

