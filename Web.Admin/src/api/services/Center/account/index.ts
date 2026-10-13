import { axiosUtil } from "@fast-china/axios";
import type { ElSelectorOutput, PagedInput, PagedResult } from "fast-element-plus";
import type { AccountIdInput } from "./models/AccountIdInput";
import type { AccountVerificationInput } from "./models/AccountVerificationInput";
import type { BindWeChatInput } from "./models/BindWeChatInput";
import type { ChangePasswordByWeChatInput } from "./models/ChangePasswordByWeChatInput";
import type { ChangePasswordInput } from "./models/ChangePasswordInput";
import type { EditAccountInput } from "./models/EditAccountInput";
import type { PasswordResetInput } from "./models/PasswordResetInput";
import type { QueryAccountDetailOutput } from "./models/QueryAccountDetailOutput";
import type { QueryAccountPagedInput } from "./models/QueryAccountPagedInput";
import type { QueryAccountPagedOutput } from "./models/QueryAccountPagedOutput";
import type { SendAccountVerificationCodeInput } from "./models/SendAccountVerificationCodeInput";
import type { SendPasswordResetCodeInput } from "./models/SendPasswordResetCodeInput";
import type { SendPasswordResetCodeOutput } from "./models/SendPasswordResetCodeOutput";

/**
 * 账号服务Api
 */
export const accountApi = {
	/**
	 * 账号选择器
	 */
	accountSelector(data: PagedInput): Promise<PagedResult<ElSelectorOutput<string>>> {
		return axiosUtil.request<PagedResult<ElSelectorOutput<string>>>({
			url: "/account/accountSelector",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取账号分页列表
	 */
	queryAccountPaged(data: QueryAccountPagedInput): Promise<PagedResult<QueryAccountPagedOutput>> {
		return axiosUtil.request<PagedResult<QueryAccountPagedOutput>>({
			url: "/account/queryAccountPaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取账号详情
	 */
	queryAccountDetail(accountId: string): Promise<QueryAccountDetailOutput> {
		return axiosUtil.request<QueryAccountDetailOutput>({
			url: "/account/queryAccountDetail",
			method: "get",
			params: {
				accountId,
			},
			requestType: "query",
		});
	},
	/**
	 * 获取编辑账号详情
	 */
	queryEditAccountDetail(): Promise<EditAccountInput> {
		return axiosUtil.request<EditAccountInput>({
			url: "/account/queryEditAccountDetail",
			method: "get",
			requestType: "query",
		});
	},
	/**
	 * 编辑账号
	 */
	editAccount(data: EditAccountInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/editAccount",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 账号解除锁定
	 */
	unlock(data: AccountIdInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/unlock",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 账号更改状态
	 */
	changeStatus(data: AccountIdInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/changeStatus",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 账号修改密码
	 */
	changePassword(data: ChangePasswordInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/changePassword",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 通过绑定的微信修改密码
	 */
	changePasswordByWeChat(data: ChangePasswordByWeChatInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/changePasswordByWeChat",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 账号重置密码
	 */
	resetPassword(data: AccountIdInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/resetPassword",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 发送密码重置验证码
	 */
	sendPasswordResetCode(data: SendPasswordResetCodeInput): Promise<SendPasswordResetCodeOutput> {
		return axiosUtil.request<SendPasswordResetCodeOutput>({
			url: "/account/sendPasswordResetCode",
			method: "post",
			data,
			requestType: "auth",
		});
	},
	/**
	 * 通过验证码重置密码
	 */
	resetPasswordByVerificationCode(data: PasswordResetInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/resetPasswordByVerificationCode",
			method: "post",
			data,
			requestType: "auth",
		});
	},
	/**
	 * 发送账号校验验证码
	 */
	sendAccountVerificationCode(data: SendAccountVerificationCodeInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/sendAccountVerificationCode",
			method: "post",
			data,
			requestType: "auth",
		});
	},
	/**
	 * 账号校验
	 */
	accountVerification(data: AccountVerificationInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/accountVerification",
			method: "post",
			data,
			requestType: "auth",
		});
	},
	/**
	 * 发送编辑账号验证码
	 */
	sendEditAccountVerificationCode(data: SendAccountVerificationCodeInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/sendEditAccountVerificationCode",
			method: "post",
			data,
			requestType: "auth",
		});
	},
	/**
	 * 绑定微信用户
	 */
	bindWeChat(data: BindWeChatInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/bindWeChat",
			method: "post",
			data,
			requestType: "auth",
		});
	},
	/**
	 * 解绑微信用户
	 */
	unBindWeChat(data: BindWeChatInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/account/unBindWeChat",
			method: "post",
			data,
			requestType: "auth",
		});
	},
};
