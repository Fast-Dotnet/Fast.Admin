import type { PagedInput } from "fast-element-plus";

/**
 * 获取文件分页列表输入
 */
export interface QueryFilePagedInput extends PagedInput  {
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

