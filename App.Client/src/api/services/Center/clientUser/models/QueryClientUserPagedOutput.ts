import type { ClientUserTypeEnum } from "@/api/enums/ClientUserTypeEnum";
import type { GenderEnum } from "@/api/enums/GenderEnum";

/**
 * 获取客户端用户分页列表输出
 */
export interface QueryClientUserPagedOutput {
	/**
	 * 客户端用户Id
	 */
	userId?: string;
	/**
	 * 应用Id
	 */
	appId?: string;
	/**
	 * 
	 */
	userType?: ClientUserTypeEnum;
	/**
	 * 手机
	 */
	mobile?: string;
	/**
	 * 唯一用户标识
	 */
	openId?: string;
	/**
	 * 统一用户标识
	 */
	unionId?: string;
	/**
	 * 昵称
	 */
	nickName?: string;
	/**
	 * 头像
	 */
	avatar?: string;
	/**
	 * 
	 */
	sex?: GenderEnum;
	/**
	 * 最后登录设备
	 */
	lastLoginDevice?: string;
	/**
	 * 最后登录操作系统（版本）
	 */
	lastLoginOS?: string;
	/**
	 * 最后登录浏览器（版本）
	 */
	lastLoginBrowser?: string;
	/**
	 * 最后登录省份
	 */
	lastLoginProvince?: string;
	/**
	 * 最后登录城市
	 */
	lastLoginCity?: string;
	/**
	 * 最后登录Ip
	 */
	lastLoginIp?: string;
	/**
	 * 最后登录时间
	 */
	lastLoginTime?: string;
	/**
	 * 手机号更新时间
	 */
	mobileUpdateTime?: string;
	/**
	 * 创建时间
	 */
	createdTime?: string;
	/**
	 * 
	 */
	createdUserName?: string;
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

