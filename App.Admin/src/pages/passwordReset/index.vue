<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" :disabled="state.loading || state.isSending" border>
			<wd-form-item title="找回方式" prop="channel">
				<wd-radio-group v-model="channel" direction="horizontal" @change="handleChannelChange">
					<wd-radio value="mobile">手机号</wd-radio>
					<wd-radio value="email">邮箱</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item :title="channel === 'mobile' ? '绑定手机' : '绑定邮箱'" prop="account" layout="vertical">
				<wd-input
					v-model="state.formData.account"
					clearable
					:inputmode="channel === 'mobile' ? 'tel' : 'email'"
					:maxlength="channel === 'mobile' ? 11 : 50"
					:placeholder="channel === 'mobile' ? '请输入账号绑定的手机号' : '请输入账号绑定的邮箱'"
				>
					<template #suffix>
						<wd-button
							custom-style="margin-left: var(--wot-spacing-tight)"
							size="small"
							type="info"
							variant="plain"
							:loading="state.isSending"
							:disabled="countdown > 0 || state.loading"
							@click="handleSend"
						>
							{{ countdown > 0 ? `${countdown} 秒后重发` : "发送验证码" }}
						</wd-button>
					</template>
				</wd-input>
			</wd-form-item>

			<FaImageCaptcha
				ref="captchaRef"
				title="验证码"
				prop="captchaCode"
				layout="vertical"
				v-model="state.formData.captchaCode"
				v-model:captcha-key="state.formData.captchaKey"
				is-force
				:disabled="state.loading || state.isSending"
			/>

			<wd-cell-group title="验证码5分钟内有效；未收到或已失效时，请重新获取。">
				<wd-form-item title="验证码" prop="verificationCode">
					<wd-input v-model="state.formData.verificationCode" inputmode="numeric" :maxlength="6" placeholder="请输入6位验证码" clearable />
				</wd-form-item>
				<wd-form-item title="新密码" prop="newPassword">
					<wd-input
						v-model="state.formData.newPassword"
						:show-word-limit="false"
						show-password
						:maxlength="20"
						placeholder="8~20位，包含大小写字母和数字"
						clearable
					/>
				</wd-form-item>
				<wd-form-item title="确认密码" prop="confirmPassword">
					<wd-input
						v-model="state.formData.confirmPassword"
						:show-word-limit="false"
						show-password
						:maxlength="20"
						placeholder="8~20位，包含大小写字母和数字"
						clearable
					/>
				</wd-form-item>
			</wd-cell-group>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" :loading="state.loading" :disabled="state.isSending" @click="handleConfirm">
			重置密码
		</wd-button>
	</view>
</template>

<script setup lang="ts">
import { computed, reactive, ref, shallowRef } from "vue";
import { throttle, useNow, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { accountApi } from "@/api/services/Center/account";
import { CommonRoute } from "@/common";
import { RegExps } from "@/constants";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { PasswordResetInput } from "@/api/services/Center/account/models/PasswordResetInput";
import type { SendPasswordResetCodeInput } from "@/api/services/Center/account/models/SendPasswordResetCodeInput";
import type { FaImageCaptchaInstance } from "@/components";

definePage({
	name: "PasswordReset",
	noLogin: true,
	authForbidView: true,
	watermark: false,
	style: {
		navigationBarTitleText: "忘记密码",
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();
const captchaRef = ref<FaImageCaptchaInstance>();

const channel = shallowRef<"email" | "mobile">("email");

const state = reactive({
	loading: false,
	isSending: false,
	nextSendAt: 0,
	formData: withDefineType<SendPasswordResetCodeInput & PasswordResetInput>({}),
	formSchema: useFormSchema<SendPasswordResetCodeInput & PasswordResetInput>({
		verificationCode: [
			{ required: true, message: "请输入验证码" },
			{ pattern: RegExps.VerificationCode, message: "请输入正确的验证码" },
		],
		newPassword: [
			{ required: true, message: "请输入新密码" },
			{ pattern: RegExps.MediumPassword, message: "密码为8~20位，必须包含大小写字母和数字" },
		],
		confirmPassword: [
			{ required: true, message: "请再次输入新密码" },
			{ validator: (value, model) => (value !== model.newPassword ? "两次密码输入不一致" : undefined) },
		],
		account: [
			{ required: true, message: "请输入账号绑定的手机号或邮箱" },
			{
				validator(value) {
					const pattern = channel.value === "mobile" ? RegExps.Mobile : RegExps.Email;
					return !pattern.test(value ?? "") ? "请输入正确的手机号或邮箱" : undefined;
				},
			},
		],
		captchaCode: [
			{ required: true, message: "请输入图形验证码" },
			{ pattern: RegExps.ImageCaptchaCode, message: "图形验证码必须为4位字母或数字" },
		],
	}),
});
const now = useNow();
/** 验证码重新发送倒计时 */
const countdown = computed(() => Math.min(60, Math.max(0, Math.ceil((state.nextSendAt - now.value.getTime()) / 1000))));

/** 切换找回方式 */
const handleChannelChange = () => {
	state.formData = {};
	formRef.value?.reset();
	captchaRef.value?.refresh();
};

/** 发送密码重置验证码 */
const handleSend = throttle(async () => {
	if (countdown.value > 0) return;
	const { valid } = await formRef.value.validate(["account", "captchaKey", "captchaCode"]);
	if (!valid) return;
	state.isSending = true;
	const { account, captchaKey, captchaCode } = state.formData;
	try {
		const apiRes = await accountApi.sendPasswordResetCode({
			account,
			captchaKey,
			captchaCode,
		});
		state.formData.verificationKey = apiRes.verificationKey;
		state.formData.verificationCode = "";
		state.nextSendAt = Date.now() + 60_000;
		useToast.success(apiRes.message);
	} finally {
		captchaRef.value?.refresh();
		state.isSending = false;
	}
});

const handleConfirm = throttle(async () => {
	const { verificationKey, verificationCode, newPassword, confirmPassword } = state.formData;
	if (!verificationKey) return;
	const { valid } = await formRef.value.validate(["verificationCode", "newPassword", "confirmPassword"]);
	if (!valid) return;
	state.loading = true;
	try {
		await accountApi.resetPasswordByVerificationCode({
			verificationKey,
			verificationCode,
			newPassword,
			confirmPassword,
		});
		await useMessageBox.alert({ msg: "密码重置成功，请使用新密码登录", confirmButtonText: "返回登录", closeOnClickModal: false });
		await router.replaceAll(CommonRoute.Login);
	} finally {
		state.loading = false;
	}
});
</script>

<style lang="scss" scoped>
.wd-cell {
	--wot-cell-vertical-padding-top: 0;
}
</style>
