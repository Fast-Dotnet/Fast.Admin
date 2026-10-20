/**
 * Sql异常日志表Model类
 */
export interface SqlExceptionLogModel {
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
	 * 文件名称
	 */
	fileName?: string;
	/**
	 * 文件行数
	 */
	fileLine?: number;
	/**
	 * 方法名
	 */
	methodName?: string;
	/**
	 * 异常信息
	 */
	message?: string;
	/**
	 * 异常源
	 */
	source?: string;
	/**
	 * 异常堆栈信息
	 */
	stackTrace?: string;
	/**
	 * 纯SQL，参数化后的SQL
	 */
	pureSql?: string;
	/**
	 * 部门Id
	 */
	departmentId?: string;
	/**
	 * 部门名称
	 */
	departmentName?: string;
	/**
	 * 异常时间
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

