import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";

/**
 * 获取账号分页列表输入
 */
export interface QueryAccountPagedInput extends PagedInput  {
	/**
	 * 手机
	 */
	mobile?: string;
	/**
	 * 邮箱
	 */
	email?: string;
	/**
	 * 
	 */
	status?: CommonStatusEnum;
	/**
	 * 初次登录城市
	 */
	firstLoginCity?: string;
	/**
	 * 初次登录Ip
	 */
	firstLoginIp?: string;
	/**
	 * 最后登录城市
	 */
	lastLoginCity?: string;
	/**
	 * 最后登录Ip
	 */
	lastLoginIp?: string;
	/**
	 * 是否锁定
	 */
	isLock?: boolean;
}

