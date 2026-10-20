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
			<FaFormItem label="应用" span="2">
				<el-input :model-value="props.appName" disabled />
			</FaFormItem>
			<FaFormItem prop="appKey" label="应用标识">
				<el-input v-model="state.formData.appKey" maxlength="50" placeholder="请输入应用标识" />
			</FaFormItem>
			<FaFormItem prop="appSecret" label="应用密钥">
				<el-input v-model="state.formData.appSecret" maxlength="50" placeholder="请输入应用密钥" />
			</FaFormItem>
			<FaFormItem prop="appType" label="应用类型" span="2">
				<RadioGroup name="AppEnvironmentEnum" v-model="state.formData.appType" />
			</FaFormItem>
			<FaFormItem prop="environmentType" label="环境类型" span="2">
				<RadioGroup name="EnvironmentTypeEnum" v-model="state.formData.environmentType" />
			</FaFormItem>
			<FaFormItem prop="loginComponent" label="登录组件" span="2">
				<el-radio-group v-model="state.formData.loginComponent" clearable placeholder="请选择登录组件">
					<el-radio label="无" value="" />
					<el-radio label="经典" value="ClassicLogin" />
					<el-radio label="现代" value="ModernLogin" />
					<el-radio label="简约" value="SimpleLogin" />
					<el-radio label="分屏" value="SplitLogin" />
				</el-radio-group>
			</FaFormItem>
			<FaFormItem prop="requestEncipher" label="请求加密">
				<el-checkbox v-model="state.formData.requestEncipher">加密</el-checkbox>
			</FaFormItem>
			<FaFormItem prop="requestTimeout" label="请求超时">
				<el-input-number v-model="state.formData.requestTimeout" :min="5000" :max="60000" placeholder="请输入请求超时时间">
					<template #suffix>毫秒</template>
				</el-input-number>
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input type="textarea" v-model="state.formData.remark" :rows="2" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">支付相关</el-divider>
			</FaLayoutGridItem>
			<FaFormItem prop="weChatMerchantId" label="微信商户号">
				<MerchantSelect
					:merchant-type="PaymentChannelEnum.WeChat"
					v-model="state.formData.weChatMerchantId"
					v-model:merchant-no="state.formData.weChatMerchantNo"
					placeholder="请选择微信商户号"
				/>
			</FaFormItem>
			<FaFormItem prop="alipayMerchantId" label="支付宝商户号">
				<MerchantSelect
					:merchant-type="PaymentChannelEnum.Alipay"
					v-model="state.formData.alipayMerchantId"
					v-model:merchant-no="state.formData.alipayMerchantNo"
					placeholder="请选择支付宝商户号"
				/>
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";
import { applicationClientApi } from "@/api/services/Center/applicationClient";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddApplicationClientInput } from "@/api/services/Center/applicationClient/models/AddApplicationClientInput";
import type { EditApplicationClientInput } from "@/api/services/Center/applicationClient/models/EditApplicationClientInput";

defineOptions({
	name: "SystemApplicationClientEdit",
});

const props = defineProps<{
	/** 应用Id */
	appId?: string;
	/** 应用名称 */
	appName?: string;
}>();

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditApplicationClientInput & AddApplicationClientInput>({}),
	formRules: withDefineType<FormRules<EditApplicationClientInput & AddApplicationClientInput>>({
		appKey: [{ required: true, message: "请输入应用标识", trigger: "blur" }],
		appType: [{ required: true, message: "请选择应用类型", trigger: "change" }],
		environmentType: [{ required: true, message: "请选择环境类型", trigger: "change" }],
		requestTimeout: [{ required: true, message: "请输入请求超时时间", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "应用标识",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		// Radio 使用空字符串表示“无”，提交时转换为接口的空值。
		const formData = {
			...state.formData,
			loginComponent: state.formData.loginComponent === "" ? null : state.formData.loginComponent,
		};
		switch (state.dialogState) {
			case "add":
				await applicationClientApi.addApplicationClient(formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await applicationClientApi.editApplicationClient(formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await applicationClientApi.queryApplicationClientDetail(recordId);
		state.formData = { ...apiRes, loginComponent: apiRes.loginComponent ?? "" };
		state.dialogTitle = `应用标识详情 - ${apiRes.appKey}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加应用标识";
		state.formDisabled = false;
		state.formData = {
			appId: props.appId,
			appType: AppEnvironmentEnum.Web,
			environmentType: EnvironmentTypeEnum.Production,
			requestTimeout: 60000,
			requestEncipher: true,
			loginComponent: "",
		};
	});
};

const edit = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await applicationClientApi.queryApplicationClientDetail(recordId);
		state.formData = { ...apiRes, loginComponent: apiRes.loginComponent ?? "" };
		state.dialogTitle = `编辑应用标识 - ${apiRes.appKey}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
