import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import type { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";

/**
 * 查询应用版本分页列表输出
 */
export interface QueryApplicationVersionPagedOutput {
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
	packageType?: ApplicationPackageTypeEnum;
	/**
	 * 版本名称
	 */
	versionName?: string;
	/**
	 * 版本号
	 */
	versionCode?: number;
	/**
	 * 
	 */
	status?: CommonStatusEnum;
	/**
	 * 最低兼容原生版本号
	 */
	minNativeVersionCode?: number;
	/**
	 * 最高兼容原生版本号
	 */
	maxNativeVersionCode?: number;
	/**
	 * 更新说明
	 */
	updateContent?: string;
	/**
	 * 是否强制更新
	 */
	isForceUpdate?: boolean;
	/**
	 * 备注
	 */
	remark?: string;
	/**
	 * 
	 */
	createdUserName?: string;
	/**
	 * 
	 */
	createdTime?: string;
	/**
	 * 
	 */
	updatedUserName?: string;
	/**
	 * 
	 */
	updatedTime?: string;
	/**
	 * 
	 */
	rowVersion?: string;
}

