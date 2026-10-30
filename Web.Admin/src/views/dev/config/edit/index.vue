<template>
	<FaDialog
		ref="faDialogRef"
		width="500"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled">
			<FaFormItem prop="configCode" label="配置编码">
				<el-input v-model="state.formData.configCode" maxlength="50" placeholder="请输入配置编码" />
			</FaFormItem>
			<FaFormItem prop="configName" label="配置名称">
				<el-input v-model="state.formData.configName" maxlength="50" placeholder="请输入配置名称" />
			</FaFormItem>
			<FaFormItem prop="configValue" label="配置值">
				<el-input type="textarea" v-model="state.formData.configValue" :rows="2" maxlength="500" placeholder="请输入配置值" />
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input type="textarea" v-model="state.formData.remark" :rows="2" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { configApi } from "@/api/services/Center/config";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddConfigInput } from "@/api/services/Center/config/models/AddConfigInput";
import type { EditConfigInput } from "@/api/services/Center/config/models/EditConfigInput";

defineOptions({
	name: "DevConfigEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditConfigInput & AddConfigInput>({}),
	formRules: withDefineType<FormRules<EditConfigInput & AddConfigInput>>({
		configCode: [{ required: true, message: "请输入配置编码", trigger: "blur" }],
		configName: [{ required: true, message: "请输入配置名称", trigger: "blur" }],
		configValue: [{ required: true, message: "请输入配置值", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "配置",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await configApi.addConfig(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await configApi.editConfig(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (configId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await configApi.queryConfigDetail(configId);
		state.formData = apiRes;
		state.dialogTitle = `配置详情 - ${apiRes.configName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加配置";
		state.formDisabled = false;
		state.formData = {};
	});
};

const edit = (configId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await configApi.queryConfigDetail(configId);
		state.formData = apiRes;
		state.dialogTitle = `编辑配置 - ${apiRes.configName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
