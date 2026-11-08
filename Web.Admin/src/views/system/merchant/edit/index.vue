<template>
	<FaDialog
		ref="faDialogRef"
		width="800"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="1">
			<FaFormItem prop="merchantType" label="商户类型">
				<RadioGroup name="PaymentChannelEnum" v-model="state.formData.merchantType" />
			</FaFormItem>
			<FaFormItem prop="merchantName" label="商户名称">
				<el-input v-model="state.formData.merchantName" maxlength="30" placeholder="请输入商户名称" />
			</FaFormItem>
			<FaFormItem prop="merchantNo" label="商户号">
				<el-input v-model="state.formData.merchantNo" maxlength="32" placeholder="请输入商户号" />
			</FaFormItem>
			<FaFormItem prop="merchantSecret" label="商户密钥">
				<el-input type="textarea" v-model="state.formData.merchantSecret" :rows="2" maxlength="200" placeholder="请输入商户密钥" />
			</FaFormItem>
			<FaFormItem prop="publicSerialNum" label="公钥序号">
				<el-input type="textarea" v-model="state.formData.publicSerialNum" :rows="2" maxlength="200" placeholder="请输入公钥序号" />
			</FaFormItem>
			<FaFormItem prop="publicKey" label="公钥">
				<el-input type="textarea" v-model="state.formData.publicKey" :rows="5" maxlength="4096" placeholder="请输入公钥" />
			</FaFormItem>
			<FaFormItem prop="certSerialNum" label="证书序号">
				<el-input type="textarea" v-model="state.formData.certSerialNum" :rows="2" maxlength="200" placeholder="请输入证书序号" />
			</FaFormItem>
			<FaFormItem prop="cert" label="证书">
				<el-input type="textarea" v-model="state.formData.cert" :rows="5" maxlength="4096" placeholder="请输入证书" />
			</FaFormItem>
			<FaFormItem prop="certPrivateKey" label="证书私钥">
				<el-input type="textarea" v-model="state.formData.certPrivateKey" :rows="5" maxlength="4096" placeholder="请输入证书私钥" />
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input type="textarea" v-model="state.formData.remark" :rows="2" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";
import { merchantApi } from "@/api/services/Center/merchant";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddMerchantInput } from "@/api/services/Center/merchant/models/AddMerchantInput";
import type { EditMerchantInput } from "@/api/services/Center/merchant/models/EditMerchantInput";

defineOptions({
	name: "DevConfigEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditMerchantInput & AddMerchantInput>({}),
	formRules: withDefineType<FormRules<EditMerchantInput & AddMerchantInput>>({
		merchantName: [{ required: true, message: "请输入商户名称", trigger: "blur" }],
		merchantNo: [{ required: true, message: "请输入商户号", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "商户号",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await merchantApi.addMerchant(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await merchantApi.editMerchant(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (merchantId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await merchantApi.queryMerchantDetail(merchantId);
		state.formData = apiRes;
		state.dialogTitle = `商户号详情 - ${apiRes.merchantNo}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加商户号";
		state.formDisabled = false;
		state.formData = {
			merchantType: PaymentChannelEnum.WeChat,
		};
	});
};

const edit = (merchantId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await merchantApi.queryMerchantDetail(merchantId);
		state.formData = apiRes;
		state.dialogTitle = `编辑商户号 - ${apiRes.merchantNo}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
