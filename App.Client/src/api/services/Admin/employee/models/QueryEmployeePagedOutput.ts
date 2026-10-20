import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { EmployeeStatusEnum } from "@/api/enums/EmployeeStatusEnum";
import type { GenderEnum } from "@/api/enums/GenderEnum";

/**
 * 获取职员分页列表输出
 */
export interface QueryEmployeePagedOutput {
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
	 * 手机
	 */
	mobile?: string;
	/**
	 * 
	 */
	status?: EmployeeStatusEnum;
	/**
	 * 邮箱
	 */
	email?: string;
	/**
	 * 
	 */
	sex?: GenderEnum;
	/**
	 * 证件照
	 */
	idPhoto?: string;
	/**
	 * 入职日期
	 */
	entryDate?: string;
	/**
	 * 离职日期
	 */
	resignDate?: string;
	/**
	 * 备注
	 */
	remark?: string;
	/**
	 * 机构Id
	 */
	orgId?: string;
	/**
	 * 机构名称
	 */
	orgName?: string;
	/**
	 * 机构名称
	 */
	orgNames?: string[];
	/**
	 * 部门Id
	 */
	departmentId?: string;
	/**
	 * 部门名称
	 */
	departmentName?: string;
	/**
	 * 部门名称
	 */
	departmentNames?: string[];
	/**
	 * 职位Id
	 */
	positionId?: string;
	/**
	 * 职位名称
	 */
	positionName?: string;
	/**
	 * 职级Id
	 */
	jobLevelId?: string;
	/**
	 * 职级名称
	 */
	jobLevelName?: string;
	/**
	 * 是否为负责人
	 */
	isPrincipal?: boolean;
	/**
	 * 角色名称
	 */
	roleNames?: string;
	/**
	 * 
	 */
	accountStatus?: CommonStatusEnum;
	/**
	 * 账号手机
	 */
	accountMobile?: string;
	/**
	 * 账号邮箱
	 */
	accountEmail?: string;
	/**
	 * 账号昵称
	 */
	accountNickName?: string;
	/**
	 * 最后登录时间
	 */
	lastLoginTime?: string;
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

