import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { EditionEnum } from "@/api/enums/EditionEnum";
import type { TenantTypeEnum } from "@/api/enums/TenantTypeEnum";

/**
 * 获取租户分页列表输出
 */
export interface QueryTenantPagedOutput {
	/**
	 * 租户Id
	 */
	tenantId?: string;
	/**
	 * 租户编号
	 */
	tenantNo?: string;
	/**
	 * 租户编码
	 */
	tenantCode?: string;
	/**
	 * 
	 */
	status?: CommonStatusEnum;
	/**
	 * 租户名称
	 */
	tenantName?: string;
	/**
	 * 租户简称
	 */
	shortName?: string;
	/**
	 * 租户英文名称
	 */
	spellName?: string;
	/**
	 * 
	 */
	edition?: EditionEnum;
	/**
	 * 租户管理员账号Id
	 */
	adminAccountId?: string;
	/**
	 * 租户管理员名称
	 */
	adminName?: string;
	/**
	 * 租户管理员手机
	 */
	adminMobile?: string;
	/**
	 * 租户管理员邮箱
	 */
	adminEmail?: string;
	/**
	 * 租户管理员电话
	 */
	adminPhone?: string;
	/**
	 * 租户机器人名称
	 */
	robotName?: string;
	/**
	 * 
	 */
	tenantType?: TenantTypeEnum;
	/**
	 * Logo URL
	 */
	logoUrl?: string;
	/**
	 * 阿里云签名名称
	 */
	aliyunSignName?: string;
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

