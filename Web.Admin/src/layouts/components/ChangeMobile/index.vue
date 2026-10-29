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
			<FaFormItem prop="mobile" label="手机">
				<el-input
					v-model="state.formData.mobile"
					maxlength="11"
					placeholder="请输入手机"
					autocapitalize="off"
					autocomplete="tel"
					inputmode="tel"
					spellcheck="false"
				>
					<template #append>
						<FaButton ref="mobileButtonRef" :disabled="!mobileChanged || mobileCountdown > 0" @click="handleSendVerificationCode">
							{{ mobileCountdown > 0 ? mobileCountdown + " 秒后重发" : "发送验证码" }}
						</FaButton>
					</template>
				</el-input>
			</FaFormItem>
			<ImageCaptcha
				ref="mobileCaptchaRef"
				prop="mobileCaptchaCode"
				v-model="state.formData.mobileCaptchaCode"
				v-model:captcha-key="state.formData.mobileCaptchaKey"
				is-force
				@keyup.enter="mobileButtonRef.doLoading(() => handleSendVerificationCode())"
			/>
			<FaFormItem prop="mobileVerificationCode" label="验证码" row style="max-width: 450px">
				<el-input
					v-model="state.formData.mobileVerificationCode"
					maxlength="6"
					placeholder="请输入 6 位短信验证码"
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
	name: "ChangeMobile",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");
const mobileButtonRef = useTemplateRef<FaButtonInstance>("mobileButtonRef");
const mobileCaptchaRef = useTemplateRef<InstanceType<typeof ImageCaptcha>>("mobileCaptchaRef");

type IFormData = EditAccountInput &
	QueryAccountDetailOutput & {
		mobileCaptchaKey?: string;
		mobileCaptchaCode?: string;
	};

const state = reactive({
	mobileNextSendAt: 0,
	initialMobile: "",
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<EditAccountInput>>({
		mobile: [
			{ required: true, message: "请输入手机", trigger: "blur" },
			{ pattern: RegExps.Mobile, message: "请输入正确的手机号", trigger: "blur" },
		],
		mobileVerificationCode: [
			{ required: true, message: "请输入短信验证码", trigger: "blur" },
			{ pattern: RegExps.VerificationCode, message: "请输入正确的短信验证码", trigger: "blur" },
		],
	}),
	dialogTitle: "修改手机号",
});

const now = useNow();
const mobileChanged = computed(() => state.formData.mobile?.trim() !== state.initialMobile);
/** 手机验证码重新发送倒计时 */
const mobileCountdown = computed(() => Math.min(60, Math.max(0, Math.ceil((state.mobileNextSendAt - now.value.getTime()) / 1000))));

/** 发送验证码 */
const handleSendVerificationCode = (_?: Event, done?: () => void) => {
	const { mobile, mobileCaptchaKey, mobileCaptchaCode } = state.formData;
	faFormRef.value.validateField(["mobile", "mobileCaptchaKey", "mobileCaptchaCode"], async (isValid) => {
		if (!isValid) {
			done?.();
			return;
		}
		await accountApi
			.sendEditAccountVerificationCode({
				account: mobile,
				captchaKey: mobileCaptchaKey,
				captchaCode: mobileCaptchaCode,
			})
			.finally(() => {
				mobileCaptchaRef.value?.refresh();
				done?.();
			});
		state.mobileNextSendAt = Date.now() + 60_000;
		state.formData.mobileVerificationCode = "";
		ElMessage.success("短信验证码已发送，5分钟内有效");
	});
};

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateField(["mobile", "mobileVerificationCode"]);
		await accountApi.editAccount(state.formData);
		ElMessage.success("保存成功！");
	});
};

const open = () => {
	faDialogRef.value.open(async () => {
		const apiRes = await accountApi.queryEditAccountDetail();
		state.formData = apiRes;
		state.initialMobile = apiRes.mobile;
		state.dialogTitle = `修改手机号 - ${apiRes.nickName}`;
	});
};

watch(
	() => state.formData.mobile,
	() => {
		state.formData.mobileVerificationCode = "";
	}
);

defineExpose({
	element: faDialogRef,
	open,
});
</script>
