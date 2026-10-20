import type { DatabaseTypeEnum } from "@/api/enums/DatabaseTypeEnum";

/**
 * 初始化数据库输入
 */
export interface InitDatabaseInput {
	/**
	 * 租户Id
	 */
	tenantId?: string;
	/**
	 * 
	 */
	databaseType?: DatabaseTypeEnum;
}

