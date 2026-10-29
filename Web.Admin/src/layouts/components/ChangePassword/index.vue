<template>
	<FaDialog
		ref="faDialogRef"
		title="修改密码"
		:show-fullscreen="false"
		:show-refresh="false"
		show-before-close
		width="450"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="1">
			<FaFormItem label="旧密码" prop="oldPassword">
				<el-input
					type="password"
					v-model="state.formData.oldPassword"
					maxlength="20"
					placeholder="请输入旧密码"
					:show-word-limit="false"
					autocomplete="current-password"
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
import { reactive, useTemplateRef, watch } from "vue";
import { ElMessageBox } from "element-plus";
import { type FaDialogInstance, type FaFormInstance, RegExps } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import { useUserInfo } from "@/stores";
import type { FormRules } from "element-plus";
import type { ChangePasswordInput } from "@/api/services/Center/account/models/ChangePasswordInput";

defineOptions({
	name: "ChangePassword",
});

const userInfoStore = useUserInfo();

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<ChangePasswordInput>({}),
	formRules: withDefineType<FormRules<ChangePasswordInput>>({
		oldPassword: [{ required: true, message: "请输入旧密码", trigger: "blur" }],
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

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await accountApi.changePassword(state.formData);
		await ElMessageBox.alert("修改成功，请重新登录！", {
			type: "success",
			confirmButtonText: "重新登录",
		});
		await userInfoStore.logout();
	});
};

const open = () => {
	faDialogRef.value.open(async () => {
		const apiRes = await accountApi.queryEditAccountDetail();
		state.formData = {
			rowVersion: apiRes.rowVersion,
		};
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
