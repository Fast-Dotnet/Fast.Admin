<template>
	<FaDialog ref="faDialogRef" width="800" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form>
			<FaFormItem label="角色名称" :value="state.formData.roleName" />
			<FaFormItem label="角色编码" :value="state.formData.roleCode" />
			<FaFormItem label="角色类型">
				<Tag name="RoleTypeEnum" :value="state.formData.roleType" />
			</FaFormItem>
			<FaFormItem label="系统菜单">
				<Tag name="BooleanEnum" :value="state.formData.isSystemMenu" />
			</FaFormItem>
			<FaFormItem label="排序" :value="state.formData.sort" />
			<FaFormItem label="数据范围">
				<Tag name="DataScopeTypeEnum" :value="state.formData.dataScopeType" />
			</FaFormItem>
			<FaFormItem v-if="state.formData.dataScopeType === DataScopeTypeEnum.CustomDept" label="自定义部门" :value="dataScopeDepartmentNames" />
			<FaFormItem label="可分配角色" :value="roleNames" />
			<FaFormItem label="备注" :value="state.formData.remark" />
			<FaFormItem label="创建人" :value="state.formData.createdUserName" />
			<FaFormItem label="创建时间" :value="state.formData.createdTime" />
			<FaFormItem label="更新人" :value="state.formData.updatedUserName" />
			<FaFormItem label="更新时间" :value="state.formData.updatedTime" />
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { computed, reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import { roleApi } from "@/api/services/Admin/role";
import type { ElSelectorOutput, FaDialogInstance } from "fast-element-plus";
import type { QueryRoleDetailOutput } from "@/api/services/Admin/role/models/QueryRoleDetailOutput";

defineOptions({
	name: "AdminRoleDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryRoleDetailOutput>({}),
	dialogTitle: "角色",
	roleList: withDefineType<ElSelectorOutput<string>[]>([]),
});

const dataScopeDepartmentNames = computed(() => state.formData.dataScopeDepartmentNames?.join("，"));
const roleNames = computed(() =>
	state.formData.assignableRoleIds?.map((roleId) => state.roleList.find((role) => role.value === roleId)?.label).join("，")
);

const detail = (roleId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await roleApi.queryRoleDetail(roleId);
		state.formData = apiRes;
		state.dialogTitle = `角色详情 - ${apiRes.roleName}`;
		state.roleList = await roleApi.roleSelector();
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
