import type { PagedInput } from "fast-element-plus";

/**
 * 获取 SQL 异常日志分页列表输入
 */
export interface QuerySqlExceptionLogPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

