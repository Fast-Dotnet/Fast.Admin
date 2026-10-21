import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import type { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import type { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";

/**
 * 获取消息模板详情输出
 */
export interface QueryMessageTemplateDetailOutput {
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 应用名称
	 */
	appName?: string;
	/**
	 * 
	 */
	appType?: AppEnvironmentEnum;
	/**
	 * 
	 */
	environmentType?: EnvironmentTypeEnum;
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 
	 */
	templateType?: MessageTemplateTypeEnum;
	/**
	 * 模板Code
	 */
	templateCode?: string;
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

