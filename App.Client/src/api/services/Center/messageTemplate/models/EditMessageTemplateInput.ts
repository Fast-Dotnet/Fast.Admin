import type { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";

/**
 * 编辑消息模板输入
 */
export interface EditMessageTemplateInput {
	/**
	 * 应用标识
	 */
	appKey?: string;
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
	rowVersion?: string;
}

