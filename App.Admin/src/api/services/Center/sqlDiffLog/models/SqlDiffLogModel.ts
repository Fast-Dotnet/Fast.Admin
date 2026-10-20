import type { DiffLogColumnInfo } from "./DiffLogColumnInfo";
import type { DiffLogTypeEnum } from "@/api/enums/DiffLogTypeEnum";

/**
 * Sql差异日志表Model类
 */
export interface SqlDiffLogModel {
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
	 * 
	 */
	diffType?: DiffLogTypeEnum;
	/**
	 * 表名称
	 */
	tableName?: string;
	/**
	 * 表描述
	 */
	tableDescription?: string;
	/**
	 * 旧的列信息
	 */
	beforeColumnList?: DiffLogColumnInfo[][];
	/**
	 * 新的列信息
	 */
	afterColumnList?: DiffLogColumnInfo[][];
	/**
	 * 执行秒数
	 */
	executeSeconds?: number;
	/**
	 * 纯SQL，参数化后的SQL
	 */
	pureSql?: string;
	/**
	 * 部门Id
	 */
	departmentId?: string;
	/**
	 * 部门名称
	 */
	departmentName?: string;
	/**
	 * 差异时间
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
	/**
	 * 
	 */
	createdUserId?: string;
	/**
	 * 
	 */
	createdUserName?: string;
}

