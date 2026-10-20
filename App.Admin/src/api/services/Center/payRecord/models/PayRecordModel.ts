import type { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";

/**
 * 支付记录表Model类
 */
export interface PayRecordModel {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 收款商户号
	 */
	merchantNo?: string;
	/**
	 * 业务订单Id
	 */
	bizOrderId?: string;
	/**
	 * 业务订单号
	 */
	bizOrderNo?: string;
	/**
	 * 
	 */
	paymentChannel?: PaymentChannelEnum;
	/**
	 * 唯一用户标识
	 */
	openId?: string;
	/**
	 * 统一用户标识
	 */
	unionId?: string;
	/**
	 * 手机
	 */
	mobile?: string;
	/**
	 * 订单金额
	 */
	orderAmount?: number;
	/**
	 * 回调地址
	 */
	notifyUrl?: string;
	/**
	 * 是否已支付
	 */
	isPaid?: boolean;
	/**
	 * 支付过期时间
	 */
	payExpireTime?: string;
	/**
	 * 是否已关闭
	 */
	isClosed?: boolean;
	/**
	 * 关闭时间
	 */
	closeTime?: string;
	/**
	 * 支付金额
	 */
	paymentAmount?: number;
	/**
	 * 交易流水号
	 */
	transactionId?: string;
	/**
	 * 支付时间
	 */
	paymentTime?: string;
	/**
	 * 设备
	 */
	device?: string;
	/**
	 * 操作系统（版本）
	 */
	os?: string;
	/**
	 * 浏览器（版本）
	 */
	browser?: string;
	/**
	 * 省份
	 */
	province?: string;
	/**
	 * 城市
	 */
	city?: string;
	/**
	 * Ip地址
	 */
	ip?: string;
	/**
	 * 创建时间
	 */
	createdTime?: string;
	/**
	 * 更新时间
	 */
	updatedTime?: string;
	/**
	 * 租户Id
	 */
	tenantId?: string;
	/**
	 * 更新版本控制字段
	 */
	rowVersion?: string;
}

