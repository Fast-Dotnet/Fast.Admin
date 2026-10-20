import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { DictionaryValueTypeEnum } from "@/api/enums/DictionaryValueTypeEnum";

/**
 * 获取字典分页列表输出
 */
export interface QueryDictionaryPagedOutput {
	/**
	 * 字典Id
	 */
	dictionaryId?: string;
	/**
	 * 字典Key
	 */
	dictionaryKey?: string;
	/**
	 * 字典名称
	 */
	dictionaryName?: string;
	/**
	 * 
	 */
	valueType?: DictionaryValueTypeEnum;
	/**
	 * Flags枚举
	 */
	hasFlags?: boolean;
	/**
	 * 
	 */
	status?: CommonStatusEnum;
	/**
	 * 备注
	 */
	remark?: string;
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

