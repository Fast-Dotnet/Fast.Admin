<template>
	<FaDialog
		ref="faDialogRef"
		width="500"
		title="添加应用凭据"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules">
			<FaFormItem prop="appKey" label="应用标识">
				<el-input v-model="state.formData.appKey" maxlength="50" placeholder="请输入应用标识" />
			</FaFormItem>
			<FaFormItem prop="tokenType" label="凭据类型">
				<RadioGroup name="ApplicationTokenTypeEnum" v-model="state.formData.tokenType" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { ApplicationTokenTypeEnum } from "@/api/enums/ApplicationTokenTypeEnum";
import { applicationTokenApi } from "@/api/services/Center/applicationToken";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddApplicationTokenInput } from "@/api/services/Center/applicationToken/models/AddApplicationTokenInput";

defineOptions({
	name: "SystemApplicationTokenEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<AddApplicationTokenInput>({}),
	formRules: withDefineType<FormRules<AddApplicationTokenInput>>({
		appKey: [{ required: true, message: "请输入应用标识", trigger: "blur" }],
		tokenType: [{ required: true, message: "请选择凭据类型", trigger: "change" }],
	}),
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await applicationTokenApi.addApplicationToken(state.formData);
		ElMessage.success("新增成功！");
		emit("ok");
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.formData = {
			tokenType: ApplicationTokenTypeEnum.AccessToken,
		};
	});
};

defineExpose({
	element: faDialogRef,
	add,
});
</script>
