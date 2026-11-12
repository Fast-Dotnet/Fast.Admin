/**
 * 获取 SQL 超时日志分页列表输入
 */
export interface QuerySqlTimeoutLogPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

