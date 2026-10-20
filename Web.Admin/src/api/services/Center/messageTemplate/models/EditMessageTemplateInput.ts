import type { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";

/**
 * 编辑消息模板输入
 */
export interface EditMessageTemplateInput {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 应用标识，留空表示全局模板
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
	/**
	 * 
	 */
	rowVersion?: string;
}

