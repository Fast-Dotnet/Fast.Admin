<template>
	<FaDialog ref="faDialogRef" width="500" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form>
			<FaFormItem label="职位名称" :value="state.formData.positionName" />
			<FaFormItem label="排序" :value="state.formData.sort" />
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
import { positionApi } from "@/api/services/Admin/position";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryPositionDetailOutput } from "@/api/services/Admin/position/models/QueryPositionDetailOutput";

defineOptions({
	name: "AdminPositionDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryPositionDetailOutput>({}),
	dialogTitle: "职位",
});

const detail = (positionId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await positionApi.queryPositionDetail(positionId);
		state.formData = apiRes;
		state.dialogTitle = `职位详情 - ${apiRes.positionName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
