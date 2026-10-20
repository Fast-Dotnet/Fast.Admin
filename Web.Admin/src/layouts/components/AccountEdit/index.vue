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
			<FaFormItem prop="lastLoginIp" label="Ip">
				<el-text type="info">{{ state.formData.lastLoginIp }}</el-text>
			</FaFormItem>
			<FaFormItem prop="lastLoginTime" label="时间">
				<template v-if="state.formData.lastLoginTime">
					{{ dayjs(state.formData.lastLoginTime).format("YYYY-MM-DD HH:mm:ss") }}
				</template>
				<template v-else>-</template>
			</FaFormItem>
			<FaFormItem prop="nickName" label="昵称">
				<el-input v-model="state.formData.nickName" maxlength="20" placeholder="请输入昵称" />
			</FaFormItem>
			<FaFormItem prop="avatar" label="头像">
				<FaUploadImage v-model="state.formData.avatar" :upload-api="fileApi.uploadAvatar" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage, dayjs } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import { fileApi } from "@/api/services/File";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { EditAccountInput } from "@/api/services/Center/account/models/EditAccountInput";
import type { QueryAccountDetailOutput } from "@/api/services/Center/account/models/QueryAccountDetailOutput";

defineOptions({
	name: "AccountEdit",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditAccountInput & QueryAccountDetailOutput>({}),
	formRules: withDefineType<FormRules<EditAccountInput>>({
		nickName: [{ required: true, message: "请输入昵称", trigger: "blur" }],
		avatar: [{ required: true, message: "请上传头像", trigger: "change" }],
	}),
	dialogTitle: "账号信息",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await accountApi.editAccount(state.formData);
		ElMessage.success("保存成功！");
	});
};

const open = () => {
	faDialogRef.value.open(async () => {
		const apiRes = await accountApi.queryEditAccountDetail();
		state.formData = apiRes;
		state.dialogTitle = `编辑账号 - ${apiRes.nickName}`;
	});
};

defineExpose({
	element: faDialogRef,
	open,
});
</script>
