import type { OperateLogTypeEnum } from "@/api/enums/OperateLogTypeEnum";

/**
 * 操作日志表Model类
 */
export interface OperateLogModel {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 工号
	 */
	employeeNo?: string;
	/**
	 * 手机
	 */
	mobile?: string;
	/**
	 * 标题
	 */
	title?: string;
	/**
	 * 
	 */
	operateType?: OperateLogTypeEnum;
	/**
	 * 业务Id
	 */
	bizId?: string;
	/**
	 * 业务编码
	 */
	bizNo?: string;
	/**
	 * 描述
	 */
	description?: string;
	/**
	 * 部门Id
	 */
	departmentId?: string;
	/**
	 * 部门名称
	 */
	departmentName?: string;
	/**
	 * 操作者用户Id
	 */
	createdUserId?: string;
	/**
	 * 操作者用户名称
	 */
	createdUserName?: string;
	/**
	 * 操作时间
	 */
	createdTime?: string;
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
}

