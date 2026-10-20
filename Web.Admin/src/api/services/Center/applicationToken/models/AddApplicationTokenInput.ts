import type { ApplicationTokenTypeEnum } from "@/api/enums/ApplicationTokenTypeEnum";

/**
 * 添加应用凭据输入
 */
export interface AddApplicationTokenInput {
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 
	 */
	tokenType?: ApplicationTokenTypeEnum;
}

