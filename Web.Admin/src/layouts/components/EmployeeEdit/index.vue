<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef?.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="2">
			<FaFormItem prop="employeeNo" label="工号">
				<el-text type="info">{{ state.formData.employeeNo }}</el-text>
			</FaFormItem>
			<FaFormItem prop="entryDate" label="入职日期">
				{{ dayjs(state.formData.entryDate).format("YYYY-MM-DD HH:mm:ss") }}
			</FaFormItem>
			<FaFormItem prop="employeeName" label="职员名称">
				<el-input v-model="state.formData.employeeName" maxlength="20" placeholder="请输入职员名称" />
			</FaFormItem>
			<FaFormItem prop="sex" label="性别">
				<RadioGroup name="GenderEnum" v-model="state.formData.sex" />
			</FaFormItem>
			<FaFormItem prop="mobile" label="手机">
				<el-input
					v-model="state.formData.mobile"
					maxlength="11"
					placeholder="请输入手机"
					autocapitalize="off"
					autocomplete="tel"
					inputmode="tel"
					spellcheck="false"
				/>
			</FaFormItem>
			<FaFormItem prop="email" label="邮箱">
				<el-input v-model="state.formData.email" maxlength="50" placeholder="请输入邮箱" />
			</FaFormItem>
			<FaFormItem prop="idPhoto" label="证件照">
				<FaUploadImage v-model="state.formData.idPhoto" :upload-api="fileApi.uploadIdPhoto" />
			</FaFormItem>

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">机构信息</el-divider>
			</FaLayoutGridItem>

			<FaLayoutGridItem span="3" style="min-height: 300px; max-height: 500px">
				<FaTable :data="state.formData.orgList" :pagination="false" :header-card="false">
					<FaTableColumn prop="orgName" label="机构" width="280" />
					<FaTableColumn prop="departmentName" label="部门" width="280" />
					<FaTableColumn prop="isPrimary" label="主部门" width="80" tag :enum="booleanEnum" />
					<FaTableColumn prop="positionName" label="职位" width="280" />
					<FaTableColumn prop="jobLevelName" label="职级" width="280" />
					<FaTableColumn prop="isPrincipal" label="负责人" width="80" tag :enum="booleanEnum" />
				</FaTable>
			</FaLayoutGridItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage, dayjs } from "element-plus";
import { type FaDialogInstance, type FaFormInstance, RegExps } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { employeeApi } from "@/api/services/Admin/employee";
import { fileApi } from "@/api/services/File";
import { useApp, useUserInfo } from "@/stores";
import type { FormRules } from "element-plus";
import type { EditEmployeeInput } from "@/api/services/Admin/employee/models/EditEmployeeInput";
import type { QueryEmployeeDetailOutput } from "@/api/services/Admin/employee/models/QueryEmployeeDetailOutput";

defineOptions({
	name: "EmployeeEdit",
});

const appStore = useApp();
const booleanEnum = appStore.getDictionary("BooleanEnum");
const userInfoStore = useUserInfo();

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditEmployeeInput & QueryEmployeeDetailOutput>({}),
	formRules: withDefineType<FormRules<EditEmployeeInput>>({
		employeeName: [{ required: true, message: "请输入职员名称", trigger: "blur" }],
		mobile: [
			{ required: true, message: "请输入手机", trigger: "blur" },
			{ pattern: RegExps.Mobile, message: "请输入正确的手机号", trigger: "blur" },
		],
		email: [
			{ required: true, message: "请输入邮箱", trigger: "blur" },
			{ pattern: RegExps.Email, message: "请输入正确的邮箱", trigger: "blur" },
			{ max: 50, message: "邮箱不能超过50位字符", trigger: "blur" },
		],
		idPhoto: [{ required: true, message: "请上传证件照", trigger: "change" }],
	}),
	dialogTitle: "职员信息",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		if (userInfoStore.isSuperAdmin || userInfoStore.isAdmin) return;
		await faFormRef.value.validateScrollToField();
		await employeeApi.editSelfEmployee(state.formData);
		ElMessage.success("保存成功！");
	});
};

const open = () => {
	if (userInfoStore.isSuperAdmin || userInfoStore.isAdmin) return;
	faDialogRef.value.open(async () => {
		const apiRes = await employeeApi.queryEmployeeDetail(userInfoStore.employeeId);
		state.formData = apiRes;
		state.dialogTitle = `编辑职员 - ${apiRes.employeeName}`;
	});
};

defineExpose({
	element: faDialogRef,
	open,
});
</script>
