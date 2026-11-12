import type { PagedInput } from "fast-element-plus";
import type { HttpRequestActionEnum } from "@/api/enums/HttpRequestActionEnum";
import type { HttpRequestMethodEnum } from "@/api/enums/HttpRequestMethodEnum";

/**
 * 获取请求日志分页列表输入
 */
export interface QueryRequestLogPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 是否执行成功
	 */
	isSuccess?: boolean;
	/**
	 * 
	 */
	operationAction?: HttpRequestActionEnum;
	/**
	 * 
	 */
	requestMethod?: HttpRequestMethodEnum;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

