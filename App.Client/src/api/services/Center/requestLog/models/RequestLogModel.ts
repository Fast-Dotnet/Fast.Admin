import type { HttpRequestActionEnum } from "@/api/enums/HttpRequestActionEnum";
import type { HttpRequestMethodEnum } from "@/api/enums/HttpRequestMethodEnum";

/**
 * 请求日志表Model类
 */
export interface RequestLogModel {
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
	 * 是否执行成功
	 */
	isSuccess?: boolean;
	/**
	 * 
	 */
	operationAction?: HttpRequestActionEnum;
	/**
	 * 操作名称
	 */
	operationName?: string;
	/**
	 * 类名
	 */
	className?: string;
	/**
	 * 方法名
	 */
	methodName?: string;
	/**
	 * 请求地址
	 */
	location?: string;
	/**
	 * 
	 */
	requestMethod?: HttpRequestMethodEnum;
	/**
	 * 请求参数
	 */
	param?: string;
	/**
	 * 返回结果
	 */
	result?: string;
	/**
	 * 耗时（毫秒）
	 */
	elapsedTime?: string;
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
}

