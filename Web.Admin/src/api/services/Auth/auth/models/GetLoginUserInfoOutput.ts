import type { AuthMenuInfoDto } from "./AuthMenuInfoDto";
import type { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import type { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";

/**
 * 获取登录用户信息输出
 */
export interface GetLoginUserInfoOutput {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 账号Key
	 */
	accountKey?: string;
	/**
	 * 手机
	 */
	mobile?: string;
	/**
	 * 昵称
	 */
	nickName?: string;
	/**
	 * 头像
	 */
	avatar?: string;
	/**
	 * 身份校验
	 */
	identityVerification?: boolean;
	/**
	 * 客户端用户Id
	 */
	clientUserId?: string;
	/**
	 * 客户端唯一用户标识
	 */
	clientUserOpenId?: string;
	/**
	 * 租户编号
	 */
	tenantNo?: string;
	/**
	 * 租户名称
	 */
	tenantName?: string;
	/**
	 * 租户简称
	 */
	shortName?: string;
	/**
	 * 租户编码
	 */
	tenantCode?: string;
	/**
	 * 租户Logo URL
	 */
	logoUrl?: string;
	/**
	 * 用户Key
	 */
	userKey?: string;
	/**
	 * 职员Id
	 */
	employeeId?: string;
	/**
	 * 工号
	 */
	employeeNo?: string;
	/**
	 * 姓名
	 */
	employeeName?: string;
	/**
	 * 部门Id
	 */
	departmentId?: string;
	/**
	 * 部门名称
	 */
	departmentName?: string;
	/**
	 * 是否超级管理员
	 */
	isSuperAdmin?: boolean;
	/**
	 * 是否管理员
	 */
	isAdmin?: boolean;
	/**
	 * 角色名称集合
	 */
	roleNameList?: string[];
	/**
	 * 
	 */
	roleType?: RoleTypeEnum;
	/**
	 * 
	 */
	dataScopeType?: DataScopeTypeEnum;
	/**
	 * 按钮编码集合
	 */
	buttonCodeList?: string[];
	/**
	 * 菜单集合
	 */
	menuList?: AuthMenuInfoDto[];
}

