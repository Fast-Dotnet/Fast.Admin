import type { DiffLogTypeEnum } from "@/api/enums/DiffLogTypeEnum";

/**
 * 获取 SQL 差异日志分页列表输入
 */
export interface QuerySqlDiffLogPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 
	 */
	diffType?: DiffLogTypeEnum;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

