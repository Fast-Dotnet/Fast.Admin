<template>
	<FaDialog
		ref="faDialogRef"
		width="500"
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled">
			<FaFormItem prop="tenantId" label="租户" tips="留空表示全局模板">
				<TenantSelectPage v-model="state.formData.tenantId" v-model:tenant-name="state.formData.tenantName" />
			</FaFormItem>
			<FaFormItem prop="templateType" label="模板类型">
				<RadioGroup name="MessageTemplateTypeEnum" v-model="state.formData.templateType" />
			</FaFormItem>
			<FaFormItem prop="templateCode" label="模板Code">
				<el-input v-model="state.formData.templateCode" maxlength="50" placeholder="请输入模板Code" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";
import { messageTemplateApi } from "@/api/services/Center/messageTemplate";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddMessageTemplateInput } from "@/api/services/Center/messageTemplate/models/AddMessageTemplateInput";
import type { EditMessageTemplateInput } from "@/api/services/Center/messageTemplate/models/EditMessageTemplateInput";

defineOptions({
	name: "SystemMessageTemplateEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditMessageTemplateInput & AddMessageTemplateInput>({}),
	formRules: withDefineType<FormRules<EditMessageTemplateInput & AddMessageTemplateInput>>({
		templateType: [{ required: true, message: "请选择模板类型", trigger: "change" }],
		templateCode: [{ required: true, message: "请输入模板Code", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "消息模板",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await messageTemplateApi.addMessageTemplate(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await messageTemplateApi.editMessageTemplate(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await messageTemplateApi.queryMessageTemplateDetail(recordId);
		state.formData = apiRes;
		state.dialogTitle = `消息模板详情 - ${apiRes.templateCode}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加消息模板";
		state.formDisabled = false;
		state.formData = {
			templateType: MessageTemplateTypeEnum.RegisterVerifyCode,
		};
	});
};

const edit = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await messageTemplateApi.queryMessageTemplateDetail(recordId);
		state.formData = apiRes;
		state.dialogTitle = `编辑消息模板 - ${apiRes.templateCode}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
