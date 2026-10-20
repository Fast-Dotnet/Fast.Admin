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
			<FaFormItem prop="parentId" label="父级">
				<FaTreeSelect
					:request-api="organizationApi.organizationSelector"
					v-model="state.formData.parentId"
					v-model:label="state.formData.parentName"
					placeholder="请选择父级机构"
					check-strictly
					filterable
					clearable
				/>
			</FaFormItem>
			<FaFormItem prop="orgName" label="机构名称">
				<el-input v-model="state.formData.orgName" maxlength="20" placeholder="请输入机构名称" />
			</FaFormItem>
			<FaFormItem prop="orgCode" label="机构编码">
				<el-input v-model="state.formData.orgCode" maxlength="30" placeholder="请输入机构编码" />
			</FaFormItem>
			<FaFormItem prop="sort" label="排序" tips="从小到大">
				<el-input-number v-model="state.formData.sort" :min="1" :max="9999" placeholder="请输入排序" />
			</FaFormItem>
			<FaFormItem prop="dataPublic" label="数据公开" tips="只有“本机构及以下数据”才生效">
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
import { organizationApi } from "@/api/services/Admin/organization";
import type { FormRules } from "element-plus";
import type { AddOrganizationInput } from "@/api/services/Admin/organization/models/AddOrganizationInput";
import type { EditOrganizationInput } from "@/api/services/Admin/organization/models/EditOrganizationInput";

defineOptions({
	name: "AdminOrgEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

type IFormData = EditOrganizationInput &
	AddOrganizationInput & {
		parentName?: string;
	};

const state = reactive({
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<IFormData>>({
		orgName: [{ required: true, message: "请输入机构名称", trigger: "blur" }],
		orgCode: [{ required: true, message: "请输入机构编码", trigger: "blur" }],
		sort: [{ required: true, message: "请输入排序", trigger: "blur" }],
		dataPublic: [{ required: true, message: "请选择数据公开", trigger: "change" }],
		email: [
			{ pattern: RegExps.Email, message: "请输入正确的邮箱", trigger: "blur" },
			{ max: 50, message: "邮箱不能超过50位字符", trigger: "blur" },
		],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "机构",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await organizationApi.addOrganization(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await organizationApi.editOrganization(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (orgId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await organizationApi.queryOrganizationDetail(orgId);
		if (apiRes.parentId === "0") {
			apiRes.parentId = undefined;
		}
		state.formData = apiRes;
		state.dialogTitle = `机构详情 - ${apiRes.orgName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加机构";
		state.formDisabled = false;
		state.formData = {
			parentId: undefined,
			parentName: undefined,
			dataPublic: false,
		};
	});
};

const edit = (orgId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await organizationApi.queryOrganizationDetail(orgId);
		if (apiRes.parentId === "0") {
			apiRes.parentId = undefined;
		}
		state.formData = apiRes;
		state.dialogTitle = `编辑机构 - ${apiRes.orgName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
