import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import type { EditionEnum } from "@/api/enums/EditionEnum";
import type { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";

/**
 * Launch 输出
 */
export interface LaunchOutput {
	/**
	 * 
	 */
	edition?: EditionEnum;
	/**
	 * 应用编号
	 */
	appNo?: string;
	/**
	 * 应用名称
	 */
	appName?: string;
	/**
	 * Logo URL
	 */
	logoUrl?: string;
	/**
	 * 主题色
	 */
	themeColor?: string;
	/**
	 * 
	 */
	appType?: AppEnvironmentEnum;
	/**
	 * 
	 */
	environmentType?: EnvironmentTypeEnum;
	/**
	 * 登录组件
	 */
	loginComponent?: string;
	/**
	 * 请求超时时间（毫秒）
	 */
	requestTimeout?: number;
	/**
	 * 请求加密
	 */
	requestEncipher?: boolean;
	/**
	 * 租户名称
	 */
	tenantName?: string;
}

