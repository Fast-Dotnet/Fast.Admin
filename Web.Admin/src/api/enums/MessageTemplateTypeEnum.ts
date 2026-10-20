/**
 * 消息模板类型枚举
 */
export enum MessageTemplateTypeEnum {
	/**
	 * 注册验证码
	 */
	RegisterVerifyCode = "1",
	/**
	 * 登录验证码
	 */
	LoginVerifyCode = "2",
	/**
	 * 密码操作验证码
	 */
	PasswordOperationVerifyCode = "3",
	/**
	 * 手机号验证码
	 */
	MobileVerifyCode = "4",
	/**
	 * 注销账号验证码
	 */
	AccountCancellationVerifyCode = "5",
}
