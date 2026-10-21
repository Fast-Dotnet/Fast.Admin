<template>
	<FaDialog
		ref="faDialogRef"
		width="800"
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="1">
			<FaFormItem prop="roleType" label="角色类型">
				<RadioGroup name="RoleTypeEnum" v-model="state.formData.roleType" />
			</FaFormItem>
			<FaFormItem prop="isSystemMenu" label="系统菜单">
				<RadioGroup name="BooleanEnum" v-model="state.formData.isSystemMenu" />
			</FaFormItem>
			<FaFormItem prop="roleName" label="角色名称">
				<el-input v-model="state.formData.roleName" maxlength="20" placeholder="请输入角色名称" />
			</FaFormItem>
			<FaFormItem prop="roleCode" label="角色编码">
				<el-input v-model="state.formData.roleCode" maxlength="30" placeholder="请输入角色编码" />
			</FaFormItem>
			<FaFormItem prop="sort" label="排序" tips="从小到大">
				<el-input-number v-model="state.formData.sort" :min="1" :max="9999" placeholder="请输入排序" />
			</FaFormItem>
			<FaFormItem prop="dataScopeType" label="数据范围">
				<RadioGroup name="DataScopeTypeEnum" v-model="state.formData.dataScopeType" />
			</FaFormItem>
			<FaFormItem v-if="state.formData.dataScopeType === DataScopeTypeEnum.CustomDept" prop="dataScopeDepartmentIds" label="自定义部门">
				<DepartmentTreeSelect v-model="state.formData.dataScopeDepartmentIds" multiple collapse-tags collapse-tags-tooltip />
			</FaFormItem>
			<FaFormItem prop="assignableRoleIds" label="可分配角色" tips="为空则代表此角色不提供角色分配权限；普通账号仅可分配勾选且自己未持有的角色">
				<el-checkbox-group v-model="state.formData.assignableRoleIds">
					<el-checkbox v-for="(item, index) of state.roleList" :key="index" :value="item.value">
						{{ item.label }}
					</el-checkbox>
				</el-checkbox-group>
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
import { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";
import { roleApi } from "@/api/services/Admin/role";
import type { FormRules } from "element-plus";
import type { ElSelectorOutput, FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddRoleInput } from "@/api/services/Admin/role/models/AddRoleInput";
import type { EditRoleInput } from "@/api/services/Admin/role/models/EditRoleInput";

defineOptions({
	name: "AdminRoleEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditRoleInput & AddRoleInput>({}),
	formRules: withDefineType<FormRules<EditRoleInput & AddRoleInput>>({
		roleName: [{ required: true, message: "请输入角色名称", trigger: "blur" }],
		roleCode: [{ required: true, message: "请输入角色编码", trigger: "blur" }],
		sort: [{ required: true, message: "请输入排序", trigger: "blur" }],
		dataScopeDepartmentIds: [{ required: true, message: "请选择自定义部门", trigger: "change" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "角色",
	roleList: withDefineType<ElSelectorOutput<string>[]>([]),
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await roleApi.addRole(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await roleApi.editRole(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (roleId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await roleApi.queryRoleDetail(roleId);
		state.formData = apiRes;
		state.dialogTitle = `角色详情 - ${apiRes.roleName}`;
		state.roleList = await roleApi.roleSelector();
	});
};

const add = () => {
	faDialogRef.value.open(async () => {
		state.dialogState = "add";
		state.dialogTitle = "添加角色";
		state.formDisabled = false;
		state.formData = {
			roleType: RoleTypeEnum.Admin,
			isSystemMenu: true,
			dataScopeType: DataScopeTypeEnum.All,
			dataScopeDepartmentIds: [],
			assignableRoleIds: [],
			sort: 1,
		};
		state.roleList = await roleApi.roleSelector();
	});
};

const edit = (roleId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await roleApi.queryRoleDetail(roleId);
		state.formData = apiRes;
		state.dialogTitle = `编辑角色 - ${apiRes.roleName}`;
		state.roleList = await roleApi.roleSelector();
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
