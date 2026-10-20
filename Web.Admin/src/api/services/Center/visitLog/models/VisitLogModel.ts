import type { VisitTypeEnum } from "@/api/enums/VisitTypeEnum";

/**
 * 访问日志表Model类
 */
export interface VisitLogModel {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 手机
	 */
	mobile?: string;
	/**
	 * 昵称
	 */
	nickName?: string;
	/**
	 * 
	 */
	visitType?: VisitTypeEnum;
	/**
	 * 部门Id
	 */
	departmentId?: string;
	/**
	 * 部门名称
	 */
	departmentName?: string;
	/**
	 * 访问时间
	 */
	createdTime?: string;
	/**
	 * 租户Id
	 */
	tenantId?: string;
	/**
	 * 租户名称
	 */
	tenantName?: string;
	/**
	 * 
	 */
	device?: string;
	/**
	 * 
	 */
	os?: string;
	/**
	 * 
	 */
	browser?: string;
	/**
	 * 
	 */
	province?: string;
	/**
	 * 
	 */
	city?: string;
	/**
	 * 
	 */
	ip?: string;
	/**
	 * 
	 */
	createdUserId?: string;
	/**
	 * 
	 */
	createdUserName?: string;
}

