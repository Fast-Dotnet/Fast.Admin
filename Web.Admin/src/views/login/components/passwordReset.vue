<template>
	<FaDialog
		ref="faDialogRef"
		title="找回密码"
		:show-fullscreen="false"
		:show-refresh="false"
		show-before-close
		width="500"
		@confirm-click="handleConfirm"
		@close="handleClose"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="1">
			<FaFormItem label="找回方式" prop="channel">
				<template #label>
					<FaFormItemTip label="找回方式" />
				</template>
				<el-radio-group v-model="state.formData.channel" @change="handleChannelChange">
					<el-radio value="mobile">手机号</el-radio>
					<el-radio value="email">邮箱</el-radio>
				</el-radio-group>
			</FaFormItem>

			<FaFormItem
				v-if="state.formData.channel === 'mobile'"
				label="绑定手机"
				prop="account"
				:rules="[
					{ required: true, message: '请输入绑定手机号', trigger: 'blur' },
					{ pattern: RegExps.Mobile, message: '请输入正确的绑定手机号', trigger: 'blur' },
				]"
			>
				<el-input
					v-model="state.formData.account"
					placeholder="请输入账号绑定的手机号"
					maxlength="11"
					:show-word-limit="false"
					autocapitalize="off"
					autocomplete="tel"
					inputmode="tel"
					spellcheck="false"
				>
					<template #append>
						<el-button :disabled="countdown > 0" @click="handleSend">
							{{ countdown > 0 ? countdown + " 秒后重发" : "发送验证码" }}
						</el-button>
					</template>
				</el-input>
			</FaFormItem>
			<FaFormItem
				v-else
				label="绑定邮箱"
				prop="account"
				:rules="[
					{ required: true, message: '请输入绑定邮箱', trigger: 'blur' },
					{ pattern: RegExps.Email, message: '请输入正确的绑定邮箱', trigger: 'blur' },
					{ max: 50, message: '邮箱不能超过50位字符', trigger: 'blur' },
				]"
			>
				<el-input
					v-model="state.formData.account"
					placeholder="请输入账号绑定的邮箱"
					maxlength="50"
					:show-word-limit="false"
					autocapitalize="off"
					autocomplete="email"
					inputmode="email"
					spellcheck="false"
				>
					<template #append>
						<el-button :disabled="countdown > 0" @click="handleSend">
							{{ countdown > 0 ? countdown + " 秒后重发" : "发送验证码" }}
						</el-button>
					</template>
				</el-input>
			</FaFormItem>

			<ImageCaptcha
				ref="captchaRef"
				prop="captchaCode"
				v-model="state.formData.captchaCode"
				v-model:captcha-key="state.formData.captchaKey"
				is-force
				@keyup.enter="handleSend"
			/>

			<el-divider content-position="left">验证码5分钟内有效；未收到或已失效时，请重新获取。</el-divider>
			<FaFormItem v-if="state.formData.channel === 'mobile'" label="验证码" prop="verificationCode">
				<el-input
					v-model="state.formData.verificationCode"
					maxlength="6"
					placeholder="请输入 6 位短信验证码"
					:show-word-limit="false"
					autocomplete="one-time-code"
					inputmode="numeric"
				/>
			</FaFormItem>
			<FaFormItem v-else label="验证码" prop="verificationCode">
				<el-input
					v-model="state.formData.verificationCode"
					maxlength="6"
					placeholder="请输入 6 位邮箱验证码"
					:show-word-limit="false"
					autocomplete="one-time-code"
					inputmode="numeric"
				/>
			</FaFormItem>

			<FaFormItem label="新密码" prop="newPassword">
				<el-input
					type="password"
					v-model="state.formData.newPassword"
					placeholder="新密码8~20位，包含大小写字母和数字"
					maxlength="20"
					show-password
					:show-word-limit="false"
					autocomplete="new-password"
				/>
			</FaFormItem>
			<FaFormItem label="确认密码" prop="confirmPassword">
				<el-input
					type="password"
					v-model="state.formData.confirmPassword"
					placeholder="确认密码8~20位，包含大小写字母和数字"
					maxlength="20"
					show-password
					:show-word-limit="false"
					autocomplete="new-password"
					@keyup.enter="handleConfirm"
				/>
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { computed, reactive, useTemplateRef, watch } from "vue";
import { ElMessage } from "element-plus";
import { RegExps } from "fast-element-plus";
import { useNow, withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import ImageCaptcha from "@/components/ImageCaptcha/index.vue";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";

defineOptions({
	name: "PasswordReset",
});

interface IPasswordResetForm {
	channel: "email" | "mobile";
	account?: string;
	captchaKey?: string;
	captchaCode?: string;
	verificationCode?: string;
	newPassword?: string;
	confirmPassword?: string;
}

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");
const captchaRef = useTemplateRef<InstanceType<typeof ImageCaptcha>>("captchaRef");

const state = reactive({
	nextSendAt: 0,
	verificationKey: "",
	formData: withDefineType<IPasswordResetForm>({
		channel: "email",
	}),
	formRules: withDefineType<FormRules<IPasswordResetForm>>({
		verificationCode: [
			{ required: true, message: "请输入验证码", trigger: "blur" },
			{ pattern: RegExps.VerificationCode, message: "请输入正确的邮箱验证码", trigger: "blur" },
		],
		newPassword: [
			{ required: true, message: "请输入新密码", trigger: "blur" },
			{ pattern: RegExps.MediumPassword, message: "新密码长度必须为8~20位，且必须包含大小写字母、数字", trigger: "blur" },
		],
		confirmPassword: [
			{ required: true, message: "请输入确认新密码", trigger: "blur" },
			{
				validator: (_rule, value, callback) => {
					if (value !== state.formData.newPassword) callback(new Error("两次密码输入不一致"));
					else callback();
				},
				trigger: "blur",
			},
		],
	}),
});
const now = useNow();
/** 验证码重新发送倒计时 */
const countdown = computed(() => Math.min(60, Math.max(0, Math.ceil((state.nextSendAt - now.value.getTime()) / 1000))));

/** 切换找回方式 */
const handleChannelChange = () => {
	state.formData.account = "";
	state.verificationKey = "";
	state.formData.verificationCode = "";
	state.formData.newPassword = "";
	state.formData.confirmPassword = "";
	captchaRef.value?.refresh();
};

/** 发送密码重置验证码 */
const handleSend = () => {
	if (countdown.value > 0) return;
	const { account, captchaKey, captchaCode } = state.formData;
	faFormRef.value.validateField(["account", "captchaKey", "captchaCode"], (isValid) => {
		if (!isValid) return;
		faDialogRef.value
			.doLoading(async () => {
				const apiRes = await accountApi.sendPasswordResetCode({
					account,
					captchaKey,
					captchaCode,
				});
				state.verificationKey = apiRes.verificationKey;
				state.formData.verificationCode = "";
				state.nextSendAt = Date.now() + 60_000;
				ElMessage.success(apiRes.message);
			})
			.finally(() => {
				captchaRef.value?.refresh();
			});
	});
};

const handleConfirm = () => {
	if (!state.verificationKey) return;
	const { verificationCode, newPassword, confirmPassword } = state.formData;
	faFormRef.value.validateField(["verificationCode", "newPassword", "confirmPassword"], (isValid) => {
		if (!isValid) return;
		faDialogRef.value.close(async () => {
			await accountApi.resetPasswordByVerificationCode({
				verificationKey: state.verificationKey,
				verificationCode,
				newPassword,
				confirmPassword,
			});
			ElMessage.success("密码重置成功，请使用新密码登录");
		});
	});
};

const handleClose = () => {
	state.nextSendAt = 0;
	faFormRef.value.resetFields();
};

const open = () => {
	faDialogRef.value.open(() => {
		handleClose();
	});
};

watch(
	() => state.formData.newPassword,
	() => {
		if (state.formData.confirmPassword) {
			faFormRef.value.validateField("confirmPassword");
		} else {
			faFormRef.value.clearValidate("confirmPassword");
		}
	}
);

defineExpose({
	open,
});
</script>
