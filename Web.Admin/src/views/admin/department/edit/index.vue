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
			<FaFormItem prop="orgId" label="机构">
				<FaTreeSelect
					:request-api="organizationApi.organizationSelector"
					v-model="state.formData.orgId"
					v-model:label="state.formData.orgName"
					placeholder="请选择机构"
					check-strictly
					filterable
					clearable
				/>
			</FaFormItem>
			<FaFormItem prop="parentId" label="父级">
				<DepartmentTreeSelect
					:org-id="state.formData.orgId"
					v-model="state.formData.parentId"
					v-model:department-name="state.formData.parentName"
					placeholder="请选择父级部门"
				/>
			</FaFormItem>
			<FaFormItem prop="departmentName" label="部门名称">
				<el-input v-model="state.formData.departmentName" maxlength="20" placeholder="请输入部门名称" />
			</FaFormItem>
			<FaFormItem prop="departmentCode" label="部门编码">
				<el-input v-model="state.formData.departmentCode" maxlength="30" placeholder="请输入部门编码" />
			</FaFormItem>
			<FaFormItem prop="sort" label="排序" tips="从小到大">
				<el-input-number v-model="state.formData.sort" :min="1" :max="9999" placeholder="请输入排序" />
			</FaFormItem>
			<FaFormItem prop="dataPublic" label="数据公开" tips="只有“本部门及以下数据”才生效">
				<RadioGroup name="BooleanEnum" v-model="state.formData.dataPublic" />
			</FaFormItem>
			<FaFormItem prop="contacts" label="联系人">
				<el-input v-model="state.formData.contacts" maxlength="20" placeholder="请输入联系人" />
			</FaFormItem>
			<FaFormItem prop="phone" label="电话">
				<el-input v-model="state.formData.phone" maxlength="20" placeholder="请输入电话" />
			</FaFormItem>
			<FaFormItem prop="email" label="邮箱">
				<el-input v-model="state.formData.email" maxlength="50" placeholder="请输入邮箱" />
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
import { type FaDialogInstance, type FaFormInstance, RegExps } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { departmentApi } from "@/api/services/Admin/department";
import { organizationApi } from "@/api/services/Admin/organization";
import type { FormRules } from "element-plus";
import type { AddDepartmentInput } from "@/api/services/Admin/department/models/AddDepartmentInput";
import type { EditDepartmentInput } from "@/api/services/Admin/department/models/EditDepartmentInput";

defineOptions({
	name: "AdminDepartmentEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

type IFormData = EditDepartmentInput &
	AddDepartmentInput & {
		orgName?: string;
		parentName?: string;
	};

const state = reactive({
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<IFormData>>({
		orgId: [{ required: true, message: "请选择机构", trigger: "change" }],
		departmentName: [{ required: true, message: "请输入部门名称", trigger: "blur" }],
		departmentCode: [{ required: true, message: "请输入部门编码", trigger: "blur" }],
		sort: [{ required: true, message: "请输入排序", trigger: "blur" }],
		dataPublic: [{ required: true, message: "请选择数据公开", trigger: "change" }],
		email: [
			{ pattern: RegExps.Email, message: "请输入正确的邮箱", trigger: "blur" },
			{ max: 50, message: "邮箱不能超过50位字符", trigger: "blur" },
		],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "部门",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await departmentApi.addDepartment(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await departmentApi.editDepartment(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (departmentId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await departmentApi.queryDepartmentDetail(departmentId);
		if (apiRes.parentId === "0") {
			apiRes.parentId = undefined;
		}
		state.formData = apiRes;
		state.dialogTitle = `部门详情 - ${apiRes.departmentName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加部门";
		state.formDisabled = false;
		state.formData = {
			parentId: undefined,
			parentName: undefined,
			dataPublic: false,
		};
	});
};

const edit = (departmentId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await departmentApi.queryDepartmentDetail(departmentId);
		if (apiRes.parentId === "0") {
			apiRes.parentId = undefined;
		}
		state.formData = apiRes;
		state.dialogTitle = `编辑部门 - ${apiRes.departmentName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
