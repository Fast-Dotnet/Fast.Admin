import type { EditionEnum } from "@/api/enums/EditionEnum";

/**
 * 获取应用详情输出
 */
export interface QueryApplicationDetailOutput {
	/**
	 * 应用Id
	 */
	appId?: string;
	/**
	 * 
	 */
	edition?: EditionEnum;
	/**
	 * 应用编号
	 */
	appNo?: string;
	/**
	 * 应用名称
	 */
	appName?: string;
	/**
	 * Logo URL
	 */
	logoUrl?: string;
	/**
	 * 主题色
	 */
	themeColor?: string;
	/**
	 * 备注
	 */
	remark?: string;
	/**
	 * 租户Id
	 */
	tenantId?: string;
	/**
	 * 租户名称
	 */
	tenantName?: string;
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

