import type { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";

/**
 * 添加应用版本输入
 */
export interface AddApplicationVersionInput {
	/**
	 * 应用标识
	 */
	appKey?: string;
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
	 * 包文件地址
	 */
	fileUrl?: string;
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
}

