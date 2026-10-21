/**
 * 获取消息模板分页列表输入
 */
export interface QueryMessageTemplatePagedInput extends PagedInput  {
	/**
	 * 租户Id
	 */
	tenantId?: string;
	/**
	 * 
	 */
	readonly isOrderBy?: boolean;
}

