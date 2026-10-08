<template>
	<FaDialog
		v-if="userInfoStore.identityVerification"
		ref="faDialogRef"
		title="账号安全校验"
		width="500"
		:show-fullscreen="false"
		:show-refresh="false"
		:show-close="false"
		:show-close-button="false"
		:close-on-click-modal="false"
		:close-on-press-escape="false"
		@confirm-click="handleConfirm"
		@close="faFormRef?.resetFields()"
	>
		<FaForm ref="faFormRef" label-position="top" :model="state.formData" :rules="state.formRules" cols="1">
			<el-divider content-position="left">账号校验，可修改手机号</el-divider>
			<FaFormItem label="手机号" prop="mobile">
				<el-input
					v-model.trim="state.formData.mobile"
					placeholder="请输入账号绑定的手机号"
					maxlength="11"
					:show-word-limit="false"
					autocapitalize="off"
					autocomplete="tel"
					inputmode="tel"
					spellcheck="false"
				>
					<template #append>
						<el-button :disabled="mobileCountdown > 0" @click="handleSend('mobile')">
							{{ mobileCountdown > 0 ? mobileCountdown + " 秒后重发" : "发送验证码" }}
						</el-button>
					</template>
				</el-input>
			</FaFormItem>
			<ImageCaptcha
				ref="mobileCaptchaRef"
				prop="mobileCaptchaCode"
				v-model="state.formData.mobileCaptchaCode"
				v-model:captcha-key="state.formData.mobileCaptchaKey"
				is-force
				@keyup.enter="handleSend('mobile')"
			/>
			<FaFormItem label="短信验证码" prop="mobileVerificationCode">
				<el-input
					v-model.trim="state.formData.mobileVerificationCode"
					placeholder="请输入 6 位短信验证码"
					maxlength="6"
					:show-word-limit="false"
					autocomplete="one-time-code"
					inputmode="numeric"
				/>
			</FaFormItem>

			<el-divider content-position="left">邮箱校验，可修改邮箱</el-divider>
			<FaFormItem label="邮箱" prop="email">
				<el-input
					v-model.trim="state.formData.email"
					placeholder="请输入邮箱"
					maxlength="50"
					:show-word-limit="false"
					autocapitalize="off"
					autocomplete="email"
					inputmode="email"
					spellcheck="false"
				>
					<template #append>
						<el-button :disabled="emailCountdown > 0" @click="handleSend('email')">
							{{ emailCountdown > 0 ? emailCountdown + " 秒后重发" : "发送验证码" }}
						</el-button>
					</template>
				</el-input>
			</FaFormItem>
			<ImageCaptcha
				ref="emailCaptchaRef"
				prop="emailCaptchaCode"
				v-model="state.formData.emailCaptchaCode"
				v-model:captcha-key="state.formData.emailCaptchaKey"
				is-force
				@keyup.enter="handleSend('email')"
			/>
			<FaFormItem label="邮箱验证码" prop="emailVerificationCode">
				<el-input
					v-model.trim="state.formData.emailVerificationCode"
					maxlength="6"
					placeholder="请输入 6 位邮箱验证码"
					:show-word-limit="false"
					autocomplete="one-time-code"
					inputmode="numeric"
				/>
			</FaFormItem>
		</FaForm>

		<template #footer>
			<el-button @click="userInfoStore.logout()">退出登录</el-button>
		</template>
	</FaDialog>
</template>

<script lang="ts" setup>
import { computed, nextTick, reactive, useTemplateRef, watch } from "vue";
import { ElMessage } from "element-plus";
import { RegExps } from "fast-element-plus";
import { useNow, withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import { useUserInfo } from "@/stores";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AccountVerificationInput } from "@/api/services/Center/account/models/AccountVerificationInput";
import type ImageCaptcha from "@/components/ImageCaptcha/index.vue";

defineOptions({
	name: "IdentityVerification",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");
const mobileCaptchaRef = useTemplateRef<InstanceType<typeof ImageCaptcha>>("mobileCaptchaRef");
const emailCaptchaRef = useTemplateRef<InstanceType<typeof ImageCaptcha>>("emailCaptchaRef");
const userInfoStore = useUserInfo();

type IFormData = AccountVerificationInput & {
	mobileCaptchaKey?: string;
	mobileCaptchaCode?: string;
	emailCaptchaKey?: string;
	emailCaptchaCode?: string;
};

const state = reactive({
	/** 下次允许发送手机验证码的时间戳，单位为毫秒 */
	mobileNextSendAt: 0,
	/** 下次允许发送邮箱验证码的时间戳，单位为毫秒 */
	emailNextSendAt: 0,
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<IFormData>>({
		mobile: [
			{ required: true, message: "请输入手机号", trigger: "blur" },
			{ pattern: RegExps.Mobile, message: "请输入正确的手机号", trigger: "blur" },
		],
		mobileVerificationCode: [
			{ required: true, message: "请输入短信验证码", trigger: "blur" },
			{ pattern: RegExps.VerificationCode, message: "请输入正确的短信验证码", trigger: "blur" },
		],
		email: [
			{ required: true, message: "请输入邮箱", trigger: "blur" },
			{ pattern: RegExps.Email, message: "请输入正确的邮箱", trigger: "blur" },
			{ max: 50, message: "邮箱不能超过50位字符", trigger: "blur" },
		],
		emailVerificationCode: [
			{ required: true, message: "请输入邮箱验证码", trigger: "blur" },
			{ pattern: RegExps.VerificationCode, message: "请输入正确的邮箱验证码", trigger: "blur" },
		],
	}),
});

const now = useNow();
/** 手机验证码重新发送倒计时 */
const mobileCountdown = computed(() => Math.min(60, Math.max(0, Math.ceil((state.mobileNextSendAt - now.value.getTime()) / 1000))));
/** 邮箱验证码重新发送倒计时 */
const emailCountdown = computed(() => Math.min(60, Math.max(0, Math.ceil((state.emailNextSendAt - now.value.getTime()) / 1000))));

/** 发送账号校验验证码 */
const handleSend = (channel: "mobile" | "email") => {
	if (channel === "mobile" && mobileCountdown.value <= 0) {
		const { mobile, mobileCaptchaKey, mobileCaptchaCode } = state.formData;
		faFormRef.value.validateField(["mobile", "mobileCaptchaKey", "mobileCaptchaCode"], (isValid) => {
			if (!isValid) return;
			faDialogRef.value
				.doLoading(async () => {
					await accountApi.sendAccountVerificationCode({
						account: mobile,
						captchaKey: mobileCaptchaKey,
						captchaCode: mobileCaptchaCode,
					});
					state.mobileNextSendAt = Date.now() + 60_000;
					state.formData.mobileVerificationCode = "";
					ElMessage.success("短信验证码已发送，5分钟内有效");
				})
				.finally(() => {
					mobileCaptchaRef.value?.refresh();
				});
		});
	} else if (channel === "email" && emailCountdown.value <= 0) {
		const { email, emailCaptchaKey, emailCaptchaCode } = state.formData;
		faFormRef.value.validateField(["email", "emailCaptchaKey", "emailCaptchaCode"], (isValid) => {
			if (!isValid) return;
			faDialogRef.value
				.doLoading(async () => {
					await accountApi.sendAccountVerificationCode({
						account: email,
						captchaKey: emailCaptchaKey,
						captchaCode: emailCaptchaCode,
					});
					state.emailNextSendAt = Date.now() + 60_000;
					state.formData.emailVerificationCode = "";
					ElMessage.success("邮箱验证码已发送，5分钟内有效");
				})
				.finally(() => {
					emailCaptchaRef.value?.refresh();
				});
		});
	}
};

/** 完成手机号和邮箱校验 */
const handleConfirm = () => {
	const { mobile, mobileVerificationCode, email, emailVerificationCode } = state.formData;
	faFormRef.value.validateField(["mobile", "mobileVerificationCode", "email", "emailVerificationCode"], (isValid) => {
		if (!isValid) return;
		faDialogRef.value.close(async () => {
			await accountApi.accountVerification({
				mobile,
				mobileVerificationCode,
				email,
				emailVerificationCode,
			});
			await userInfoStore.refreshUserInfo();
			ElMessage.success("账号校验成功");
		});
	});
};

// 账号需要安全校验时，等待弹窗挂载后加载当前联系方式。
watch(
	() => userInfoStore.identityVerification,
	(newValue) => {
		if (!newValue) return;
		nextTick(() => {
			faDialogRef.value.open(async () => {
				const apiRes = await accountApi.queryEditAccountDetail();
				state.formData.mobile = apiRes.mobile;
				state.formData.email = apiRes.email;
			});
		});
	},
	{ immediate: true }
);
</script>
