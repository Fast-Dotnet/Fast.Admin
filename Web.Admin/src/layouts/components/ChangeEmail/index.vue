<template>
	<FaDialog
		ref="faDialogRef"
		width="500"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef?.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules">
			<FaFormItem prop="nickName" label="昵称">
				<el-text type="info">{{ state.formData.nickName }}</el-text>
			</FaFormItem>
			<FaFormItem prop="email" label="邮箱">
				<el-input v-model.trim="state.formData.email" maxlength="50" placeholder="请输入邮箱">
					<template #append>
						<FaButton ref="emailButtonRef" :disabled="!emailChanged || emailCountdown > 0" @click="handleSendVerificationCode">
							{{ emailCountdown > 0 ? emailCountdown + " 秒后重发" : "发送验证码" }}
						</FaButton>
					</template>
				</el-input>
			</FaFormItem>
			<ImageCaptcha
				ref="emailCaptchaRef"
				prop="emailCaptchaCode"
				v-model="state.formData.emailCaptchaCode"
				v-model:captcha-key="state.formData.emailCaptchaKey"
				is-force
				@keyup.enter="emailButtonRef.doLoading(() => handleSendVerificationCode())"
			/>
			<FaFormItem prop="emailVerificationCode" label="验证码" row style="max-width: 450px">
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
	</FaDialog>
</template>

<script lang="ts" setup>
import { computed, reactive, useTemplateRef, watch } from "vue";
import { ElMessage } from "element-plus";
import { type FaButtonInstance, type FaDialogInstance, type FaFormInstance, RegExps } from "fast-element-plus";
import { useNow, withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import type { FormRules } from "element-plus";
import type { EditAccountInput } from "@/api/services/Center/account/models/EditAccountInput";
import type { QueryAccountDetailOutput } from "@/api/services/Center/account/models/QueryAccountDetailOutput";
import type { ImageCaptcha } from "@/components";

defineOptions({
	name: "ChangeEmail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");
const emailButtonRef = useTemplateRef<FaButtonInstance>("emailButtonRef");
const emailCaptchaRef = useTemplateRef<InstanceType<typeof ImageCaptcha>>("emailCaptchaRef");

type IFormData = EditAccountInput &
	QueryAccountDetailOutput & {
		emailCaptchaKey?: string;
		emailCaptchaCode?: string;
	};

const state = reactive({
	emailNextSendAt: 0,
	initialEmail: "",
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<EditAccountInput>>({
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
	dialogTitle: "修改邮箱",
});

const now = useNow();
const emailChanged = computed(() => state.formData.email?.trim().toLowerCase() !== state.initialEmail);
/** 邮箱验证码重新发送倒计时 */
const emailCountdown = computed(() => Math.min(60, Math.max(0, Math.ceil((state.emailNextSendAt - now.value.getTime()) / 1000))));

/** 发送验证码 */
const handleSendVerificationCode = (_?: Event, done?: () => void) => {
	const { email, emailCaptchaKey, emailCaptchaCode } = state.formData;
	faFormRef.value.validateField(["email", "emailCaptchaKey", "emailCaptchaCode"], async (isValid) => {
		if (!isValid) {
			done?.();
			return;
		}
		await accountApi
			.sendEditAccountVerificationCode({
				account: email,
				captchaKey: emailCaptchaKey,
				captchaCode: emailCaptchaCode,
			})
			.finally(() => {
				emailCaptchaRef.value?.refresh();
				done?.();
			});
		state.emailNextSendAt = Date.now() + 60_000;
		state.formData.emailVerificationCode = "";
		ElMessage.success("邮箱验证码已发送，5分钟内有效");
	});
};

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateField(["email", "emailVerificationCode"]);
		await accountApi.editAccount(state.formData);
		ElMessage.success("保存成功！");
	});
};

const open = () => {
	faDialogRef.value.open(async () => {
		const apiRes = await accountApi.queryEditAccountDetail();
		state.formData = apiRes;
		state.initialEmail = apiRes.email.toLowerCase();
		state.dialogTitle = `修改邮箱 - ${apiRes.nickName}`;
	});
};

watch(
	() => state.formData.email,
	() => {
		state.formData.emailVerificationCode = "";
	}
);

defineExpose({
	element: faDialogRef,
	open,
});
</script>
