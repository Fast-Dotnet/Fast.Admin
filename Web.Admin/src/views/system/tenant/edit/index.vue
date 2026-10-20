<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="2">
			<FaFormItem prop="tenantName" label="租户名称" span="2">
				<el-input v-model="state.formData.tenantName" maxlength="30" placeholder="请输入租户名称" @input="handleSpellName" />
			</FaFormItem>
			<FaFormItem prop="tenantCode" label="租户编码" tips="单号生成前缀">
				<el-input v-model="state.formData.tenantCode" maxlength="5" placeholder="请输入租户编码" />
			</FaFormItem>
			<FaFormItem prop="shortName" label="租户简称">
				<el-input v-model="state.formData.shortName" maxlength="20" placeholder="请输入租户简称" />
			</FaFormItem>
			<FaFormItem prop="spellName" label="租户英文名称" span="2">
				<el-input v-model="state.formData.spellName" maxlength="100" placeholder="请输入租户英文名称" />
			</FaFormItem>
			<FaFormItem prop="edition" label="版本" span="2">
				<RadioGroup name="EditionEnum" v-model="state.formData.edition" />
			</FaFormItem>
			<FaFormItem prop="adminName" label="管理员名称">
				<el-input v-model="state.formData.adminName" maxlength="20" placeholder="请输入管理员名称" />
			</FaFormItem>
			<FaFormItem prop="adminMobile" label="管理员手机">
				<el-input
					v-model="state.formData.adminMobile"
					maxlength="11"
					placeholder="请输入管理员手机"
					autocapitalize="off"
					autocomplete="tel"
					inputmode="tel"
					spellcheck="false"
				/>
			</FaFormItem>
			<FaFormItem prop="adminEmail" label="管理员邮箱">
				<el-input
					v-model="state.formData.adminEmail"
					maxlength="50"
					placeholder="请输入管理员邮箱"
					autocapitalize="off"
					autocomplete="email"
					inputmode="email"
					spellcheck="false"
				/>
			</FaFormItem>
			<FaFormItem prop="adminPhone" label="管理员电话">
				<el-input v-model="state.formData.adminPhone" maxlength="20" placeholder="请输入管理员电话" />
			</FaFormItem>
			<FaFormItem prop="robotName" label="机器人名称">
				<el-input v-model="state.formData.robotName" maxlength="20" placeholder="请输入机器人名称" />
			</FaFormItem>
			<FaFormItem prop="aliyunSignName" label="阿里云签名名称">
				<el-input v-model="state.formData.aliyunSignName" maxlength="30" placeholder="请输入阿里云签名名称" />
			</FaFormItem>
			<FaFormItem v-if="state.dialogState !== 'add'" prop="status" label="状态">
				<RadioGroup button name="CommonStatusEnum" v-model="state.formData.status" />
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
import { pinyin } from "pinyin-pro";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { tenantApi } from "@/api/services/Center/tenant";
import { fileApi } from "@/api/services/File";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddTenantInput } from "@/api/services/Center/tenant/models/AddTenantInput";
import type { EditTenantInput } from "@/api/services/Center/tenant/models/EditTenantInput";

defineOptions({
	name: "SystemTenantEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditTenantInput & AddTenantInput>({}),
	formRules: withDefineType<FormRules<EditTenantInput & AddTenantInput>>({
		tenantName: [{ required: true, message: "请输入租户名称", trigger: "blur" }],
		tenantCode: [{ required: true, message: "请输入租户编码", trigger: "blur" }],
		shortName: [{ required: true, message: "请输入租户简称", trigger: "blur" }],
		spellName: [{ required: true, message: "请输入租户英文名称", trigger: "blur" }],
		adminName: [{ required: true, message: "请输入管理员名称", trigger: "blur" }],
		adminMobile: [{ required: true, message: "请输入管理员手机", trigger: "blur" }],
		adminEmail: [{ required: true, message: "请输入管理员邮箱", trigger: "blur" }],
		robotName: [{ required: true, message: "请输入机器人名称", trigger: "blur" }],
		logoUrl: [{ required: true, message: "请上传Logo", trigger: "change" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "租户",
});

const handleSpellName = (value: string) => {
	state.formData.spellName = pinyin(value, { toneType: "none", type: "array" })
		.map((item) => item.charAt(0).toUpperCase() + item.slice(1))
		.join("");
};

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await tenantApi.addTenant(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await tenantApi.editTenant(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (tenantId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await tenantApi.queryTenantDetail(tenantId);
		state.formData = apiRes;
		state.dialogTitle = `租户详情 - ${apiRes.tenantName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加租户";
		state.formDisabled = false;
		state.formData = {
			edition: EditionEnum.Trial,
			status: CommonStatusEnum.Enable,
		};
	});
};

const edit = (tenantId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await tenantApi.queryTenantDetail(tenantId);
		state.formData = apiRes;
		state.dialogTitle = `编辑租户 - ${apiRes.tenantName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
