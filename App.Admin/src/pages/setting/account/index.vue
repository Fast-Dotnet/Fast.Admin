<template>
	<view class="page">
		<wd-cell-group custom-class="top-cell-group" border>
			<!-- #ifdef MP-WEIXIN -->
			<wd-cell
				:title="userInfoStore.clientUserId ? '解除绑定' : '绑定微信'"
				:label="userInfoStore.clientUserId ? '解除当前账号与微信的绑定关系' : '绑定微信账号，支持微信快捷登录'"
				clickable
				is-link
				@click="wechatPopupRef?.open()"
			/>
			<!-- #endif -->

			<wd-cell title="修改密码" clickable is-link to="/pages/setting/account/changePassword/index" />
			<wd-cell
				v-if="userInfoStore.clientUserId"
				title="修改密码"
				label="通过已绑定的微信账号授权修改密码"
				clickable
				is-link
				to="/pages/setting/account/changePasswordByWeChat/index"
			/>
		</wd-cell-group>

		<wd-divider />

		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-cell-group>
				<wd-form-item title="头像" prop="avatar" is-link>
					<!-- #ifdef MP-WEIXIN -->
					<button class="btn__avatar" :plain="true" openType="chooseAvatar" @chooseavatar="handleAvatar">
						<image class="image__avatar" :src="state.formData.avatar" />
					</button>
					<!-- #endif -->
					<!-- #ifndef MP-WEIXIN -->
					<image class="image__avatar" :src="state.formData.avatar" @click="handleAvatar" />
					<!-- #endif -->
				</wd-form-item>

				<wd-form-item title="昵称" prop="nickName">
					<wd-input v-model="state.formData.nickName" clearable type="nickname" placeholder="请输入昵称" />
				</wd-form-item>

				<!-- #ifdef MP-WEIXIN -->
				<wd-form-item
					title="手机号"
					prop="mobile"
					:value="state.formData.mobile"
					placeholder="暂未授权手机号"
					clickable
					is-link
					@click="mobilePopupRef?.open"
				/>
				<!-- #endif -->
				<!-- #ifndef MP-WEIXIN -->
				<wd-form-item title="手机号" prop="mobile">
					<wd-input v-model="state.formData.mobile" clearable type="text" inputmode="tel" :maxlength="11" placeholder="请输入手机号" />
				</wd-form-item>
				<!-- #endif -->

				<wd-form-item title="邮箱" prop="email">
					<wd-input v-model="state.formData.email" clearable type="text" inputmode="email" placeholder="请输入邮箱" />
				</wd-form-item>
			</wd-cell-group>

			<wd-button custom-class="btn__confirm" type="danger" @click="handleConfirm">保存</wd-button>
		</wd-form>

		<!-- #ifdef MP-WEIXIN -->
		<FaPopup ref="wechatPopupRef" width="80%" :close-on-click-modal="false">
			<view class="auth-popup">
				<image class="auth-popup__logo" :src="appStore.logoUrl || defaultLogo" mode="aspectFit" />
				<view class="auth-popup__title">{{ userInfoStore.clientUserId ? "解除微信绑定" : "绑定微信" }}</view>
				<view class="auth-popup__description">
					{{
						userInfoStore.clientUserId
							? "解除绑定后，将无法通过当前微信快捷登录此管理账号。"
							: "将当前微信与此管理账号绑定，绑定后可使用微信快捷登录。"
					}}
				</view>
				<view class="auth-popup__actions">
					<wd-button type="info" block variant="plain" @click="wechatPopupRef?.close()">取消</wd-button>
					<wd-button v-if="userInfoStore.clientUserId" open-type="getUserInfo" type="danger" block @getuserinfo="handleUnBindWechat">
						确认解绑
					</wd-button>
					<wd-button v-else open-type="getUserInfo" type="primary" block @getuserinfo="handleBindWechat">确认绑定</wd-button>
				</view>
			</view>
		</FaPopup>
		<!-- #endif -->

		<!-- #ifdef MP-WEIXIN -->
		<FaPopup ref="mobilePopupRef" width="80%" :close-on-click-modal="false">
			<view class="auth-popup">
				<image class="auth-popup__logo" :src="appStore.logoUrl || defaultLogo" mode="aspectFit" />
				<view class="auth-popup__title">{{ state.formData.mobile ? "更换手机号" : "获取手机号" }}</view>
				<view class="auth-popup__description">通过微信授权获取手机号，填写到个人资料后，请点击页面底部“保存”完成修改。</view>
				<view class="auth-popup__actions">
					<wd-button type="info" block variant="plain" @click="mobilePopupRef?.close()">取消</wd-button>
					<wd-button open-type="getRealtimePhoneNumber" type="primary" block @getrealtimephonenumber="handlePhone">
						{{ state.formData.mobile ? "获取新手机号" : "授权手机号" }}
					</wd-button>
				</view>
			</view>
		</FaPopup>
		<!-- #endif -->
	</view>
</template>

<script setup lang="ts">
import { onLoad } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { logger, throttle, withDefineType } from "@fast-china/utils";
import { weChatApi } from "@/api/services/Auth/weChat";
import { accountApi } from "@/api/services/Center/account";
import { fileApi } from "@/api/services/File";
import { RegExps } from "@/constants";
import { useFormSchema, useLoading, useToast } from "@/hooks";
import defaultLogo from "@/static/logo.png";
import { useApp, useUserInfo } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { EditAccountInput } from "@/api/services/Center/account/models/EditAccountInput";
import type { FaPopupInstance } from "@/components";

definePage({
	name: "SettingAccount",
	layout: "layout",
	style: {
		navigationBarTitleText: "账号安全",
	},
});

const appStore = useApp();
const userInfoStore = useUserInfo();

const formRef = ref<FormInstance>();
const wechatPopupRef = ref<FaPopupInstance>();
const mobilePopupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<EditAccountInput>({}),
	formSchema: useFormSchema<EditAccountInput>({
		mobile: [
			{ required: true, message: "请输入手机号" },
			{ pattern: RegExps.Mobile, message: "请输入有效的手机号" },
		],
		mobileVerificationCode: [
			{ required: true, message: "请输入短信验证码" },
			{ pattern: RegExps.VerificationCode, message: "请输入正确的短信验证码" },
		],
		email: [
			{ required: true, message: "请输入邮箱" },
			{ pattern: RegExps.Email, message: "请输入正确的邮箱" },
		],
		emailVerificationCode: [
			{ required: true, message: "请输入邮箱验证码" },
			{ pattern: RegExps.VerificationCode, message: "请输入正确的邮箱验证码" },
		],
		nickName: { required: true, message: "请输入昵称" },
		avatar: { required: true, message: "请选择头像" },
	}),
});

/** 绑定微信 */
const handleBindWechat = async ({ detail }: { detail: UniNamespace.GetUserInfoRes }) => {
	const { iv, encryptedData, userInfo } = detail;
	if (!userInfo) {
		useToast.warning("绑定失败，无法获取您的信息。请重新绑定。");
		return;
	}
	await wechatPopupRef.value?.close(async () => {
		const weChatCode = await userInfoStore.getWeChatCode();
		if (!weChatCode) return;
		const { rowVersion } = state.formData;
		await accountApi.bindWeChat({ weChatCode, iv, encryptedData, rowVersion });
		useToast.success("绑定成功！");
		// 刷新用户信息
		await userInfoStore.refreshUserInfo();
	});
};

/** 解绑微信 */
const handleUnBindWechat = async ({ detail }: { detail: UniNamespace.GetUserInfoRes }) => {
	const { iv, encryptedData, userInfo } = detail;
	if (!userInfo) {
		useToast.warning("解绑失败，无法获取您的信息。请重新解绑。");
		return;
	}
	await wechatPopupRef.value?.close(async () => {
		const weChatCode = await userInfoStore.getWeChatCode();
		if (!weChatCode) return;
		const { rowVersion } = state.formData;
		await accountApi.unBindWeChat({ weChatCode, iv, encryptedData, rowVersion });
		useToast.success("解绑成功！");
		// 刷新用户信息
		await userInfoStore.refreshUserInfo();
	});
};

/** 头像上传 */
const handleAvatarUpload = async (url: string) => {
	const avatar = await fileApi.uploadAvatar(url);
	if (!avatar) return;
	state.formData.avatar = avatar;
};

/** 处理头像 */
const handleAvatar = throttle(async (event?: UniHelper.ButtonOnChooseavatarEvent) => {
	// #ifdef MP-WEIXIN
	const { avatarUrl } = event.detail;
	if (!avatarUrl) {
		useToast.error("头像获取失败");
		return;
	}
	await handleAvatarUpload(avatarUrl);
	//#endif

	// #ifndef MP-WEIXIN
	const result = await uni.chooseImage({
		count: 1,
		sizeType: ["compressed"],
	});
	await handleAvatarUpload(result.tempFilePaths[0]);
	//#endif
});

/** 处理手机号 */
const handlePhone = async ({ detail }: { detail: UniHelper.ButtonOnGetrealtimephonenumberDetail }) => {
	// #ifdef MP-WEIXIN
	logger.log("GetRealtimePhoneNumber", detail);
	const { code } = detail;
	if (!code) return;
	await mobilePopupRef.value?.close(async () => {
		const apiRes = await weChatApi.weChatCode2PhoneNumber({ code });
		state.formData.mobile = apiRes.purePhoneNumber;
		useToast.success("手机号已获取，请点击保存");
	});
	//#endif
};

/** 提交 */
const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
	await accountApi.editAccount(state.formData).finally(() => useLoading.hide());
	useToast.success("修改成功！");
	// 刷新用户信息
	await userInfoStore.refreshUserInfo();
});

onLoad(() => {
	useLoading.show();
	accountApi
		.queryEditAccountDetail()
		.then((apiRes) => {
			state.formData = apiRes;
		})
		.finally(() => useLoading.hide());
});
</script>

<style lang="scss" scoped>
.top-cell-group {
	--wot-cell-arrow-size: 16px;
}

.top-cell-group :deep(.wd-cell__arrow-right) {
	--wot-cell-arrow-margin-left: 0;
}

.top-cell-group :deep(.wd-cell__right) {
	flex: unset;
}

:deep(.wd-cell__wrapper) {
	align-items: center;
}

.btn__avatar {
	width: 100%;
	display: inline-flex;
	justify-content: right;
	padding: 0;
	background: transparent;
	border: 0;
}

.image__avatar {
	width: 120rpx;
	height: 120rpx;
	border-radius: 50%;
}

.btn__confirm {
	display: block;
	margin: 30rpx 50rpx;
}

:deep(.wd-cell__label) {
	font-size: 12px;
}

.auth-popup {
	padding: 40rpx 32rpx 32rpx;
	text-align: center;
}

.auth-popup__logo {
	width: 96rpx;
	height: 96rpx;
	border-radius: 24rpx;
}

.auth-popup__title {
	margin-top: 20rpx;
	font-size: 34rpx;
	font-weight: var(--wot-font-weight-semibold);
	color: var(--wot-text-main);
}

.auth-popup__description {
	margin-top: 18rpx;
	font-size: 26rpx;
	line-height: 1.7;
	text-align: left;
	color: var(--wot-text-secondary);
}

.auth-popup__actions {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 20rpx;
	margin-top: 32rpx;
}
</style>
