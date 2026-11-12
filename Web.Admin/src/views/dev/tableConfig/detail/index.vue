<template>
	<FaDialog ref="faDialogRef" width="500" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form>
			<FaFormItem label="表格名称" :value="state.formData.tableName" />
			<FaFormItem label="备注" :value="state.formData.remark" />
			<FaFormItem label="创建人" :value="state.formData.createdUserName" />
			<FaFormItem label="创建时间" :value="state.formData.createdTime" />
			<FaFormItem label="更新人" :value="state.formData.updatedUserName" />
			<FaFormItem label="更新时间" :value="state.formData.updatedTime" />
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { tableApi } from "@/api/services/Center/table";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryTableConfigDetailOutput } from "@/api/services/Center/table/models/QueryTableConfigDetailOutput";

defineOptions({
	name: "DevTableConfigDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryTableConfigDetailOutput>({}),
	dialogTitle: "表格",
});

const detail = (tableId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await tableApi.queryTableConfigDetail(tableId);
		state.formData = apiRes;
		state.dialogTitle = `表格详情 - ${apiRes.tableName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
