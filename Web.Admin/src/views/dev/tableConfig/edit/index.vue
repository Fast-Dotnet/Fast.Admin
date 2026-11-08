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
			<FaFormItem prop="tableName" label="表格名称">
				<el-input v-model="state.formData.tableName" maxlength="50" placeholder="请输入表格名称" />
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input v-model="state.formData.remark" type="textarea" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { tableApi } from "@/api/services/Center/table";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddTableConfigInput } from "@/api/services/Center/table/models/AddTableConfigInput";
import type { EditTableConfigInput } from "@/api/services/Center/table/models/EditTableConfigInput";

defineOptions({
	name: "DevTableConfigEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditTableConfigInput & AddTableConfigInput>({}),
	formRules: withDefineType<FormRules<EditTableConfigInput & AddTableConfigInput>>({
		tableName: [{ required: true, message: "请输入表格名称", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "表格",
	copyTableId: withDefineType<string>(undefined),
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await tableApi.addTableConfig(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await tableApi.editTableConfig(state.formData);
				ElMessage.success("保存成功！");
				break;
			case "copy":
				await tableApi.copyTableConfig({
					...state.formData,
					tableId: state.copyTableId,
				});
				ElMessage.success("复制成功");
				break;
		}
		emit("ok");
	});
};

const detail = (tableId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await tableApi.queryTableConfigDetail(tableId);
		state.formData = apiRes;
		state.dialogTitle = `表格详情 - ${apiRes.tableName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加表格";
		state.formDisabled = false;
		state.formData = {};
	});
};

const edit = (tableId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await tableApi.queryTableConfigDetail(tableId);
		state.formData = apiRes;
		state.dialogTitle = `编辑表格 - ${apiRes.tableName}`;
	});
};

const copy = (tableId: string) => {
	faDialogRef.value.open(() => {
		state.copyTableId = tableId;
		state.dialogState = "copy";
		state.formDisabled = false;
		state.dialogTitle = "复制表格配置";
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
	copy,
});
</script>
