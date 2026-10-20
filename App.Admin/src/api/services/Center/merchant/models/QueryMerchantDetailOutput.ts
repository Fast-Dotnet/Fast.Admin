import type { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";

/**
 * 获取商户号详情输出
 */
export interface QueryMerchantDetailOutput {
	/**
	 * 商户号Id
	 */
	merchantId?: string;
	/**
	 * 
	 */
	merchantType?: PaymentChannelEnum;
	/**
	 * 商户名称
	 */
	merchantName?: string;
	/**
	 * 商户号
	 */
	merchantNo?: string;
	/**
	 * 商户密钥
	 */
	merchantSecret?: string;
	/**
	 * 公钥序号
	 */
	publicSerialNum?: string;
	/**
	 * 公钥
	 */
	publicKey?: string;
	/**
	 * 证书序号
	 */
	certSerialNum?: string;
	/**
	 * 证书
	 */
	cert?: string;
	/**
	 * 证书私钥
	 */
	certPrivateKey?: string;
	/**
	 * 备注
	 */
	remark?: string;
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

