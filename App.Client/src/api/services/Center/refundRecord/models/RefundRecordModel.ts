import type { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";

/**
 * 退款记录表Model类
 */
export interface RefundRecordModel {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 退款商户号
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
	 * 是否已退款
	 */
	isRefunded?: boolean;
	/**
	 * 交易流水号
	 */
	refundId?: string;
	/**
	 * 退款金额
	 */
	refundAmount?: number;
	/**
	 * 退款时间
	 */
	refundTime?: string;
	/**
	 * 退款状态
	 */
	refundStatus?: string;
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

