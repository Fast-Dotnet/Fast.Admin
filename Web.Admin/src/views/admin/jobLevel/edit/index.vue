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
			<FaFormItem prop="jobLevelName" label="职级名称">
				<el-input v-model="state.formData.jobLevelName" maxlength="20" placeholder="请输入职级名称" />
			</FaFormItem>
			<FaFormItem prop="level" label="职级等级" tips="T:技术职级; M:管理职级; C:综合职级; 数字越大 职级越高">
				<el-input v-model="state.formData.level" maxlength="5" placeholder="请输入职级等级" />
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
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddJobLevelInput } from "@/api/services/Admin/jobLevel/models/AddJobLevelInput";
import type { EditJobLevelInput } from "@/api/services/Admin/jobLevel/models/EditJobLevelInput";

defineOptions({
	name: "AdminJobLevelEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditJobLevelInput & AddJobLevelInput>({}),
	formRules: withDefineType<FormRules<EditJobLevelInput & AddJobLevelInput>>({
		jobLevelName: [{ required: true, message: "请输入职级名称", trigger: "blur" }],
		level: [{ required: true, message: "请输入职级等级", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "职级",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await jobLevelApi.addJobLevel(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await jobLevelApi.editJobLevel(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (jobLevelId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await jobLevelApi.queryJobLevelDetail(jobLevelId);
		state.formData = apiRes;
		state.dialogTitle = `职级详情 - ${apiRes.jobLevelName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加职级";
		state.formDisabled = false;
		state.formData = {};
	});
};

const edit = (jobLevelId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await jobLevelApi.queryJobLevelDetail(jobLevelId);
		state.formData = apiRes;
		state.dialogTitle = `编辑职级 - ${apiRes.jobLevelName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
