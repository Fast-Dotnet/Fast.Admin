import type { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";

/**
 * 检查应用更新包输出
 */
export interface CheckAppUpdatePackageOutput {
	/**
	 * 版本记录Id
	 */
	recordId?: string;
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
	 * 是否强制更新
	 */
	isForceUpdate?: boolean;
	/**
	 * 更新说明
	 */
	updateContent?: string;
	/**
	 * 文件名称
	 */
	fileName?: string;
	/**
	 * 文件大小，单位为 KB（向上取整）
	 */
	fileSizeKb?: string;
	/**
	 * 文件 SHA-256，小写十六进制
	 */
	fileSha256?: string;
	/**
	 * 下载地址，支持 GET 和 Range
	 */
	downloadUrl?: string;
}

