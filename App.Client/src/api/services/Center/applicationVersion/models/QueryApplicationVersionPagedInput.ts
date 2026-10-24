import type { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";

/**
 * 查询应用版本分页列表输入
 */
export interface QueryApplicationVersionPagedInput extends PagedInput  {
	/**
	 * 
	 */
	packageType?: ApplicationPackageTypeEnum;
	/**
	 * 
	 */
	status?: CommonStatusEnum;
	/**
	 * 
	 */
	readonly isOrderBy?: boolean;
}

