/**
 * 通过已绑定微信修改密码输入
 */
export interface ChangePasswordByWeChatInput {
	/**
	 * wx.login 获取到的Code
	 */
	weChatCode?: string;
	/**
	 * 新密码
	 */
	newPassword?: string;
	/**
	 * 确认密码
	 */
	confirmPassword?: string;
	/**
	 * 
	 */
	rowVersion?: string;
}

