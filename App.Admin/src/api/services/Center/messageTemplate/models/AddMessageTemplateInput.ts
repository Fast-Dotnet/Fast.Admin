import type { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";

/**
 * 添加消息模板输入
 */
export interface AddMessageTemplateInput {
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 
	 */
	templateType?: MessageTemplateTypeEnum;
	/**
	 * 模板Code
	 */
	templateCode?: string;
}

