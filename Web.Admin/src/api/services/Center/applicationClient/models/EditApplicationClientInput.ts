import type { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import type { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";

/**
 * 编辑应用标识输入
 */
export interface EditApplicationClientInput {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 应用Id
	 */
	appId?: string;
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 
	 */
	appType?: AppEnvironmentEnum;
	/**
	 * 应用密钥
	 */
	appSecret?: string;
	/**
	 * 
	 */
	environmentType?: EnvironmentTypeEnum;
	/**
	 * 登录组件
	 */
	loginComponent?: string;
	/**
	 * 请求超时时间（毫秒）
	 */
	requestTimeout?: number;
	/**
	 * 请求加密
	 */
	requestEncipher?: boolean;
	/**
	 * 微信商户号Id
	 */
	weChatMerchantId?: string;
	/**
	 * 微信商户号
	 */
	weChatMerchantNo?: string;
	/**
	 * 支付宝商户号Id
	 */
	alipayMerchantId?: string;
	/**
	 * 支付宝商户号
	 */
	alipayMerchantNo?: string;
	/**
	 * 备注
	 */
	remark?: string;
	/**
	 * 
	 */
	rowVersion?: string;
}

