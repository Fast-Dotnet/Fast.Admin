import type { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";

/**
 * 获取商户号分页列表输出
 */
export interface QueryMerchantPagedOutput {
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

