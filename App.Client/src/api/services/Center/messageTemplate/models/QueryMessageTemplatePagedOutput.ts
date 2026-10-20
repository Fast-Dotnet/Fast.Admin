import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import type { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import type { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";

/**
 * 获取消息模板分页列表输出
 */
export interface QueryMessageTemplatePagedOutput {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 应用名称
	 */
	appName?: string;
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 
	 */
	appType?: AppEnvironmentEnum;
	/**
	 * 
	 */
	environmentType?: EnvironmentTypeEnum;
	/**
	 * 
	 */
	templateType?: MessageTemplateTypeEnum;
	/**
	 * 模板Code
	 */
	templateCode?: string;
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

