import type { PagedInput } from "fast-element-plus";
import type { DatabaseTypeEnum } from "@/api/enums/DatabaseTypeEnum";
import type { SugarDbType } from "@/api/enums/SugarDbType";

/**
 * 获取数据库分页列表输入
 */
export interface QueryDatabasePagedInput extends PagedInput  {
	/**
	 * 
	 */
	databaseType?: DatabaseTypeEnum;
	/**
	 * 
	 */
	dbType?: SugarDbType;
	/**
	 * 租户Id
	 */
	tenantId?: string;
}

