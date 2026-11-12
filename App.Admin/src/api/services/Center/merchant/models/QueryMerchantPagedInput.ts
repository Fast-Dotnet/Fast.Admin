import type { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";

/**
 * 获取商户号分页列表输入
 */
export interface QueryMerchantPagedInput extends PagedInput  {
	/**
	 * 
	 */
	merchantType?: PaymentChannelEnum;
}

