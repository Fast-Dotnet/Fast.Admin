<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="2">
			<FaFormItem prop="edition" label="版本" span="2">
				<RadioGroup name="EditionEnum" v-model="state.formData.edition" />
			</FaFormItem>
			<FaFormItem prop="appName" label="应用名称">
				<el-input v-model="state.formData.appName" maxlength="30" placeholder="请输入应用名称" />
			</FaFormItem>
			<FaFormItem prop="themeColor" label="主题色">
				<ColorPicker style="width: 32px" v-model="state.formData.themeColor" />
			</FaFormItem>
			<FaFormItem prop="tenantId" label="租户" span="2">
				<TenantSelectPage v-model="state.formData.tenantId" v-model:tenant-name="state.formData.tenantName" />
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input type="textarea" v-model="state.formData.remark" :rows="2" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
			<FaFormItem prop="logoUrl" label="Logo">
				<FaUploadImage v-model="state.formData.logoUrl" :upload-api="fileApi.uploadLogo" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { applicationApi } from "@/api/services/Center/application";
import { fileApi } from "@/api/services/File";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddApplicationInput } from "@/api/services/Center/application/models/AddApplicationInput";
import type { EditApplicationInput } from "@/api/services/Center/application/models/EditApplicationInput";

defineOptions({
	name: "SystemApplicationEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditApplicationInput & AddApplicationInput>({}),
	formRules: withDefineType<FormRules<EditApplicationInput & AddApplicationInput>>({
		appName: [{ required: true, message: "请输入应用名称", trigger: "blur" }],
		themeColor: [{ required: true, message: "请选择主题色", trigger: "blur" }],
		logoUrl: [{ required: true, message: "请上传Logo", trigger: "change" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "应用",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await applicationApi.addApplication(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await applicationApi.editApplication(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (appId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await applicationApi.queryApplicationDetail(appId);
		state.formData = apiRes;
		state.dialogTitle = `应用详情 - ${apiRes.appName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加应用";
		state.formDisabled = false;
		state.formData = {
			edition: EditionEnum.None,
		};
	});
};

const edit = (appId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await applicationApi.queryApplicationDetail(appId);
		state.formData = apiRes;
		state.dialogTitle = `编辑应用 - ${apiRes.appName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
