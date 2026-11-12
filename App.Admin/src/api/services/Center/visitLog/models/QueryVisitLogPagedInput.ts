import type { VisitTypeEnum } from "@/api/enums/VisitTypeEnum";

/**
 * 获取访问日志分页列表输入
 */
export interface QueryVisitLogPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 
	 */
	visitType?: VisitTypeEnum;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

