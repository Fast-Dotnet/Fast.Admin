import type { DatabaseTypeEnum } from "@/api/enums/DatabaseTypeEnum";
import type { SugarDbType } from "@/api/enums/SugarDbType";

/**
 * 获取数据库分页列表输出
 */
export interface QueryDatabasePagedOutput {
	/**
	 * 主库Id
	 */
	mainId?: string;
	/**
	 * 
	 */
	databaseType?: DatabaseTypeEnum;
	/**
	 * 
	 */
	dbType?: SugarDbType;
	/**
	 * 公网Ip地址
	 */
	publicIp?: string;
	/**
	 * 内网Ip地址
	 */
	intranetIp?: string;
	/**
	 * 端口号
	 */
	port?: number;
	/**
	 * 数据库名称
	 */
	dbName?: string;
	/**
	 * 数据库用户
	 */
	dbUser?: string;
	/**
	 * 超时时间，单位秒
	 */
	commandTimeOut?: number;
	/**
	 * SqlSugar SQL 执行最大秒数，如果超过记录警告日志
	 */
	sugarSqlExecMaxSeconds?: number;
	/**
	 * 差异日志
	 */
	diffLog?: boolean;
	/**
	 * 禁用 SqlSugar 的 AOP
	 */
	disableAop?: boolean;
	/**
	 * 是否初始化
	 */
	isInitialized?: boolean;
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

