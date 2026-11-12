/**
 * 获取异常日志分页列表输入
 */
export interface QueryExceptionLogPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

