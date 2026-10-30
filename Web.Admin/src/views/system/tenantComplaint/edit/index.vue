<template>
	<FaDialog
		ref="faDialogRef"
		width="600"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="1">
			<FaFormItem prop="handleDescription" label="处理描述">
				<el-input type="textarea" v-model="state.formData.handleDescription" :rows="2" maxlength="200" placeholder="请输入处理描述" />
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
import { complaintApi } from "@/api/services/Center/complaint";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { HandleComplaintInput } from "@/api/services/Center/complaint/models/HandleComplaintInput";

defineOptions({
	name: "SystemTenantComplaintEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<HandleComplaintInput>({}),
	formRules: withDefineType<FormRules<HandleComplaintInput>>({
		handleDescription: [{ required: true, message: "请输入处理描述", trigger: "blur" }],
	}),
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "处理投诉",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await complaintApi.handleComplaint(state.formData);
		ElMessage.success("新增成功！");
		emit("ok");
	});
};

const open = (complaintId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await complaintApi.queryComplaintDetail(complaintId);
		state.formData = {
			complaintId: apiRes.complaintId,
			rowVersion: apiRes.rowVersion,
		};
		state.dialogTitle = `处理投诉 - ${apiRes.nickName}`;
	});
};

defineExpose({
	element: faDialogRef,
	open,
});
</script>
