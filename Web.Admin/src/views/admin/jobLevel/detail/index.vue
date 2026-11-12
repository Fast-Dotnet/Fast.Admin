<template>
	<FaDialog ref="faDialogRef" width="500" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form>
			<FaFormItem label="职级名称" :value="state.formData.jobLevelName" />
			<FaFormItem label="职级等级" :value="state.formData.level" />
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
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryJobLevelDetailOutput } from "@/api/services/Admin/jobLevel/models/QueryJobLevelDetailOutput";

defineOptions({
	name: "AdminJobLevelDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryJobLevelDetailOutput>({}),
	dialogTitle: "职级",
});

const detail = (jobLevelId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await jobLevelApi.queryJobLevelDetail(jobLevelId);
		state.formData = apiRes;
		state.dialogTitle = `职级详情 - ${apiRes.jobLevelName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
