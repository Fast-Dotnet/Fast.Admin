/**
 * 获取 SQL 执行日志分页列表输入
 */
export interface QuerySqlExecutionLogPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

