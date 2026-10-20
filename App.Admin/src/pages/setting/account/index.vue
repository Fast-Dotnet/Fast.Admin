<template>
	<view class="page">
		<wd-cell-group custom-class="top-cell-group" border>
			<!-- #ifdef MP-WEIXIN -->
			<wd-cell v-if="state.formData.clientUserId" title="解除绑定" label="解除当前账号与微信的绑定关系" clickable is-link>
				<button class="btn__bind" :plain="true" openType="getUserInfo" @getuserinfo="handleUnBindWechat" />
			</wd-cell>
			<wd-cell v-else title="绑定微信" label="绑定微信账号，支持微信快捷登录" clickable is-link>
				<button class="btn__bind" :plain="true" openType="getUserInfo" @getuserinfo="handleBindWechat" />
			</wd-cell>
			<!-- #endif -->

			<wd-cell title="修改密码" clickable is-link to="/pages/setting/account/changePassword/index" />
			<wd-cell
				v-if="state.formData.clientUserId"
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

				<wd-form-item title="手机号" prop="mobile">
					<wd-input v-model="state.formData.mobile" clearable type="tel" :maxlength="11" placeholder="请输入手机号" />
				</wd-form-item>

				<wd-form-item title="邮箱" prop="email">
					<wd-input v-model="state.formData.email" clearable type="text" inputmode="email" placeholder="请输入邮箱" />
				</wd-form-item>
			</wd-cell-group>

			<wd-button custom-class="btn__confirm" type="danger" @click="handleConfirm">保存</wd-button>
		</wd-form>
	</view>
</template>

<script setup lang="ts">
import { onLoad } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import { fileApi } from "@/api/services/File";
import { RegExps } from "@/constants";
import { useFormSchema, useLoading, useToast } from "@/hooks";
import { useUserInfo } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { EditAccountInput } from "@/api/services/Center/account/models/EditAccountInput";

definePage({
	name: "SettingAccount",
	layout: "layout",
	style: {
		navigationBarTitleText: "账号安全",
	},
});

const userInfoStore = useUserInfo();

const formRef = ref<FormInstance>();

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
const handleBindWechat = throttle(async (event: UniHelper.ButtonOnContactEvent) => {
	const { iv, encryptedData, userInfo } = event.detail as UniNamespace.GetUserInfoRes;
	const { rowVersion } = state.formData;
	if (userInfo) {
		const weChatCode = await userInfoStore.getWeChatCode();
		if (weChatCode) {
			useLoading.show();
			await accountApi.bindWeChat({ weChatCode, iv, encryptedData, rowVersion }).finally(() => useLoading.hide());
			useToast.success("绑定成功！");
			// 刷新用户信息
			await userInfoStore.refreshUserInfo();
			return;
		}
	}
	useToast.warning("绑定失败，无法获取您的信息。请重新绑定。");
});

/** 解绑微信 */
const handleUnBindWechat = throttle(async (event: UniHelper.ButtonOnContactEvent) => {
	const { iv, encryptedData, userInfo } = event.detail as UniNamespace.GetUserInfoRes;
	const { rowVersion } = state.formData;
	if (userInfo) {
		const weChatCode = await userInfoStore.getWeChatCode();
		if (weChatCode) {
			useLoading.show();
			await accountApi.unBindWeChat({ weChatCode, iv, encryptedData, rowVersion }).finally(() => useLoading.hide());
			useToast.success("解绑成功！");
			// 刷新用户信息
			await userInfoStore.refreshUserInfo();
			return;
		}
	}
	useToast.warning("解绑失败，无法获取您的信息。请重新解绑。");
});

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
	uni.chooseImage({
		count: 1,
		sizeType: ["compressed"],
		success: async (res) => {
			await handleAvatarUpload(res.tempFilePaths[0]);
		},
	});
	//#endif
});

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

:deep(.wd-cell__wrapper) {
	align-items: center;
}

.btn__bind {
	height: 44px;
}

.btn__bind,
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
</style>
