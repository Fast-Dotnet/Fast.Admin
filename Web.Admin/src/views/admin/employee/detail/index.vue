<template>
	<FaDialog ref="faDialogRef" width="1000" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<el-row>
			<el-col :span="4" style="text-align: center">
				<FaImage style="height: 100px; width: 100px" :src="state.formData.idPhoto" />
			</el-col>

			<el-col :span="20">
				<FaForm :model="state.formData" detail-form cols="2">
					<FaFormItem label="职员名称" :value="state.formData.employeeName" />
					<FaFormItem label="工号">
						<el-text type="primary">{{ state.formData.employeeNo }}</el-text>
					</FaFormItem>
					<FaFormItem label="手机" :value="state.formData.mobile" />
					<FaFormItem label="邮箱" :value="state.formData.email" />
					<FaFormItem label="性别">
						<Tag name="GenderEnum" :value="state.formData.sex" />
					</FaFormItem>
					<FaFormItem label="状态">
						<Tag name="EmployeeStatusEnum" :value="state.formData.status" />
					</FaFormItem>
					<FaFormItem label="入职日期" :value="state.formData.entryDate?.slice(0, 10)" />
					<FaFormItem label="备注" :value="state.formData.remark" />
					<FaFormItem label="角色" :value="roleNames" />
				</FaForm>
			</el-col>
		</el-row>

		<el-divider content-position="left">其他信息</el-divider>
		<FaForm :model="state.formData" detail-form cols="2">
			<FaFormItem label="创建人" :value="state.formData.createdUserName" />
			<FaFormItem label="创建时间" :value="state.formData.createdTime" />
			<FaFormItem label="更新人" :value="state.formData.updatedUserName" />
			<FaFormItem label="更新时间" :value="state.formData.updatedTime" />
		</FaForm>

		<el-divider content-position="left">组织架构</el-divider>
		<FaTable style="height: 300px" :data="state.formData.orgList" :pagination="false" :header-card="false">
			<FaTableColumn prop="isPrimary" label="主部门" width="80" tag :enum="booleanEnum" />
			<FaTableColumn prop="orgName" label="机构" width="200" />
			<FaTableColumn prop="departmentName" label="部门" width="180" />
			<FaTableColumn prop="isPrincipal" label="负责人" width="80" tag :enum="booleanEnum" />
			<FaTableColumn prop="positionName" label="职位" width="180" />
			<FaTableColumn prop="jobLevelName" label="职级" width="180" />
		</FaTable>
	</FaDialog>
</template>

<script setup lang="ts">
import { computed, reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { employeeApi } from "@/api/services/Admin/employee";
import { useApp } from "@/stores";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryEmployeeDetailOutput } from "@/api/services/Admin/employee/models/QueryEmployeeDetailOutput";

defineOptions({
	name: "AdminEmployeeDetail",
});

const appStore = useApp();

const booleanEnum = appStore.getDictionary("BooleanEnum");

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryEmployeeDetailOutput>({}),
	dialogTitle: "职员",
});

const roleNames = computed(() => state.formData.roleList?.map((item) => item.roleName).join("，"));

const detail = (employeeId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await employeeApi.queryEmployeeDetail(employeeId);
		state.formData = apiRes;
		state.dialogTitle = `职员详情 - ${apiRes.employeeName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
