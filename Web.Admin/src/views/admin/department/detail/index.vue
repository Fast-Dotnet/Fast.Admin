<template>
	<FaDialog ref="faDialogRef" width="800" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form>
			<FaFormItem label="机构" :value="state.formData.orgName" />
			<FaFormItem label="父级" :value="state.formData.parentName" />
			<FaFormItem label="部门名称" :value="state.formData.departmentName" />
			<FaFormItem label="部门编码" :value="state.formData.departmentCode" />
			<FaFormItem label="排序" :value="state.formData.sort" />
			<FaFormItem label="数据公开">
				<Tag name="BooleanEnum" :value="state.formData.dataPublic" />
			</FaFormItem>
			<FaFormItem label="联系人" :value="state.formData.contacts" />
			<FaFormItem label="电话" :value="state.formData.phone" />
			<FaFormItem label="邮箱" :value="state.formData.email" />
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
import { departmentApi } from "@/api/services/Admin/department";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryDepartmentDetailOutput } from "@/api/services/Admin/department/models/QueryDepartmentDetailOutput";

defineOptions({
	name: "AdminDepartmentDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryDepartmentDetailOutput>({}),
	dialogTitle: "部门",
});

const detail = (departmentId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await departmentApi.queryDepartmentDetail(departmentId);
		state.formData = apiRes;
		state.dialogTitle = `部门详情 - ${apiRes.departmentName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
