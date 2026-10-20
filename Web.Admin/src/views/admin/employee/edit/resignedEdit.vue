<template>
	<FaDialog ref="faDialogRef" width="500" :title="state.dialogTitle" @confirm-click="handleConfirm" @close="faFormRef.resetFields()">
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules">
			<el-alert style="margin-bottom: 16px" type="warning" :closable="false" show-icon>
				离职后将禁用该职员的登录资格，并强制下线其全部在线会话。
			</el-alert>
			<FaFormItem prop="resignDate" label="离职日期">
				<el-date-picker
					type="date"
					v-model="state.formData.resignDate"
					:disabled-date="isDateAfterNow"
					value-format="YYYY-MM-DD"
					placeholder="请选择离职日期"
				/>
			</FaFormItem>
			<FaFormItem prop="resignReason" label="离职原因">
				<el-input type="textarea" v-model="state.formData.resignReason" :rows="2" maxlength="200" placeholder="请输入离职原因" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage, dayjs } from "element-plus";
import { isDateAfterNow, withDefineType } from "@fast-china/utils";
import { employeeApi } from "@/api/services/Admin/employee";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { EmployeeResignedInput } from "@/api/services/Admin/employee/models/EmployeeResignedInput";

defineOptions({
	name: "AdminEmployeeResignedEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EmployeeResignedInput>({
		resignDate: dayjs().format("YYYY-MM-DD"),
	}),
	formRules: withDefineType<FormRules<EmployeeResignedInput>>({
		resignDate: [{ required: true, message: "请选择离职日期", trigger: "change" }],
		resignReason: [{ required: true, message: "请输入离职原因", trigger: "blur" }],
	}),
	dialogTitle: "职员离职",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await employeeApi.employeeResigned(state.formData);
		ElMessage.success("离职成功！");
		emit("ok");
	});
};

const open = (employeeId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await employeeApi.queryEmployeeDetail(employeeId);
		state.formData = {
			employeeId: apiRes.employeeId,
			resignDate: dayjs().format("YYYY-MM-DD"),
			rowVersion: apiRes.rowVersion,
		};
		state.dialogTitle = `职员离职 - ${apiRes.employeeName}`;
	});
};

defineExpose({
	element: faDialogRef,
	open,
});
</script>
