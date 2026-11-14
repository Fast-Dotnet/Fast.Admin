import type { MessageSendChannelEnum } from "@/api/enums/MessageSendChannelEnum";

/**
 * 获取消息发送记录详情输出
 */
export interface QueryMessageSendRecordDetailOutput {
	/**
	 * 记录Id
	 */
	recordId?: string;
	/**
	 * 
	 */
	channel?: MessageSendChannelEnum;
	/**
	 * 收件人
	 */
	receiver?: string;
	/**
	 * 标题
	 */
	title?: string;
	/**
	 * 记录值
	 */
	recordValue?: string;
	/**
	 * 是否成功
	 */
	isSuccess?: boolean;
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
	 * Ip
	 */
	ip?: string;
	/**
	 * 创建人
	 */
	createdUserName?: string;
	/**
	 * 创建时间
	 */
	createdTime?: string;
}

