import type { EditDictionaryItemInput } from "./EditDictionaryItemInput";
import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { DictionaryValueTypeEnum } from "@/api/enums/DictionaryValueTypeEnum";

/**
 * 获取字典详情输出
 */
export interface QueryDictionaryDetailOutput {
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
	 * 字典项集合
	 */
	dictionaryItemList?: EditDictionaryItemInput[];
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

