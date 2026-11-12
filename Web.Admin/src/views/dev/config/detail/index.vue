<template>
	<FaDialog ref="faDialogRef" width="500" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form>
			<FaFormItem label="配置编码" :value="state.formData.configCode" />
			<FaFormItem label="配置名称" :value="state.formData.configName" />
			<FaFormItem label="配置值">
				<el-text type="primary">{{ state.formData.configValue }}</el-text>
			</FaFormItem>
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
import { configApi } from "@/api/services/Center/config";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryConfigDetailOutput } from "@/api/services/Center/config/models/QueryConfigDetailOutput";

defineOptions({
	name: "DevConfigDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryConfigDetailOutput>({}),
	dialogTitle: "配置",
});

const detail = (configId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await configApi.queryConfigDetail(configId);
		state.formData = apiRes;
		state.dialogTitle = `配置详情 - ${apiRes.configName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
