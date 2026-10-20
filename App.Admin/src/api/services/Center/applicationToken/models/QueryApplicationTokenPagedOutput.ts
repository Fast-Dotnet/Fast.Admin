import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import type { ApplicationTokenTypeEnum } from "@/api/enums/ApplicationTokenTypeEnum";
import type { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";

/**
 * 获取应用凭据分页列表输出
 */
export interface QueryApplicationTokenPagedOutput {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 应用名称
	 */
	appName?: string;
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 
	 */
	appType?: AppEnvironmentEnum;
	/**
	 * 
	 */
	environmentType?: EnvironmentTypeEnum;
	/**
	 * 
	 */
	tokenType?: ApplicationTokenTypeEnum;
	/**
	 * 刷新时间
	 */
	refreshTime?: string;
	/**
	 * 过期时间
	 */
	expireTime?: string;
}

