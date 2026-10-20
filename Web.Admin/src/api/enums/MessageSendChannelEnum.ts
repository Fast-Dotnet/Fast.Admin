/**
 * 消息发送渠道枚举
 */
export enum MessageSendChannelEnum {
	/**
	 * 邮件
	 */
	Email = 1,
	/**
	 * 短信
	 */
	Sms = 2,
	/**
	 * 微信服务号订阅消息
	 */
	WeChatServiceAccountSubscribe = 3,
	/**
	 * 微信公众号模板消息
	 */
	WeChatOfficialAccountTemplate = 4,
	/**
	 * 微信小程序订阅消息
	 */
	WeChatMiniProgramSubscribe = 5,
}
