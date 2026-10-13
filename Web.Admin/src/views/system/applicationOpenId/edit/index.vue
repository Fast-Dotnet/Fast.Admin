<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		full-height
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="2">
			<FaFormItem prop="appId" label="应用">
				<ApplicationSelect v-model="state.formData.appId" v-model:app-name="state.formData.appName" />
			</FaFormItem>
			<FaFormItem prop="openId" label="应用标识">
				<el-input v-model="state.formData.openId" maxlength="50" placeholder="请输入应用标识" />
			</FaFormItem>
			<FaFormItem prop="appType" label="应用类型">
				<FaSelect :data="appEnvironmentEnum" v-model="state.formData.appType" />
			</FaFormItem>
			<FaFormItem prop="openSecret" label="开放平台密钥">
				<el-input v-model="state.formData.openSecret" maxlength="50" placeholder="请输入开放平台密钥" />
			</FaFormItem>
			<FaFormItem prop="environmentType" label="环境类型" span="2">
				<RadioGroup name="EnvironmentTypeEnum" v-model="state.formData.environmentType" />
			</FaFormItem>
			<FaFormItem prop="loginComponent" label="登录组件">
				<el-radio-group v-model="state.formData.loginComponent">
					<el-radio value="">无</el-radio>
					<el-radio value="ClassicLogin">经典</el-radio>
					<el-radio value="ModernLogin">现代</el-radio>
					<el-radio value="SimpleLogin">简约</el-radio>
					<el-radio value="SplitLogin">分屏</el-radio>
				</el-radio-group>
			</FaFormItem>
			<FaFormItem prop="webSocketUrl" label="WebSocket地址">
				<el-input v-model="state.formData.webSocketUrl" maxlength="50" placeholder="请输入WebSocket地址" />
			</FaFormItem>
			<FaFormItem prop="requestEncipher" label="请求加密">
				<el-checkbox v-model="state.formData.requestEncipher">加密</el-checkbox>
			</FaFormItem>
			<FaFormItem prop="requestTimeout" label="请求超时时间">
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
					maxlength="20"
					placeholder="请选择微信商户号"
				/>
			</FaFormItem>
			<FaFormItem prop="alipayMerchantId" label="支付宝商户号">
				<MerchantSelect
					:merchant-type="PaymentChannelEnum.Alipay"
					v-model="state.formData.alipayMerchantId"
					v-model:merchant-no="state.formData.alipayMerchantNo"
					maxlength="20"
					placeholder="请选择支付宝商户号"
				/>
			</FaFormItem>

			<template v-if="state.dialogState !== 'add'">
				<FaLayoutGridItem span="2">
					<el-divider content-position="left">模板Id</el-divider>
				</FaLayoutGridItem>
				<FaLayoutGridItem span="2" style="min-height: 300px; max-height: 500px">
					<FaTable row-key="buttonId" :data="state.formData.templateIdList">
						<!-- 表格按钮操作区域 -->
						<template #header>
							<el-button type="primary" :icon="Plus" @click="handleTemplateIdAdd">新增</el-button>
						</template>
						<FaTableColumn prop="templateId" label="模板Id" width="400">
							<template #default="{ row, $index }: { row: EditApplicationTemplateIdInput; $index: number }">
								<el-form-item
									:prop="`templateIdList.${$index}.templateId`"
									:rules="[{ required: true, message: '请输入模板Id', trigger: 'blur' }]"
								>
									<el-input v-model="row.templateId" maxlength="50" placeholder="请输入模板Id" />
								</el-form-item>
							</template>
						</FaTableColumn>
						<FaTableColumn prop="templateType" label="模板类型" width="180">
							<template #default="{ row, $index }: { row: EditApplicationTemplateIdInput; $index: number }">
								<el-form-item
									:prop="`templateIdList.${$index}.templateType`"
									:rules="[{ required: true, message: '请选择模板类型', trigger: 'change' }]"
								>
									<FaSelect :data="applicationTemplateTypeEnum" v-model="row.templateType" />
								</el-form-item>
							</template>
						</FaTableColumn>
						<!-- 表格操作 -->
						<template #operation="{ $index }: { $index: number }">
							<el-button size="small" plain type="danger" @click="handleTemplateIdDelete($index)">删除</el-button>
						</template>
					</FaTable>
				</FaLayoutGridItem>
			</template>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";
import { applicationOpenIdApi } from "@/api/services/Center/applicationOpenId";
import { useApp } from "@/stores";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddApplicationOpenIdInput } from "@/api/services/Center/applicationOpenId/models/AddApplicationOpenIdInput";
import type { EditApplicationOpenIdInput } from "@/api/services/Center/applicationOpenId/models/EditApplicationOpenIdInput";
import type { EditApplicationTemplateIdInput } from "@/api/services/Center/applicationOpenId/models/EditApplicationTemplateIdInput";

defineOptions({
	name: "SystemApplicationOpenIdEdit",
});

const emit = defineEmits(["ok"]);

const appStore = useApp();
const appEnvironmentEnum = appStore.getDictionary("AppEnvironmentEnum");
const applicationTemplateTypeEnum = appStore.getDictionary("ApplicationTemplateTypeEnum");

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

type IFormData = EditApplicationOpenIdInput &
	AddApplicationOpenIdInput & {
		appName?: string;
		templateIdList?: EditApplicationTemplateIdInput[];
	};

const state = reactive({
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<IFormData>>({
		appId: [{ required: true, message: "请选择应用", trigger: "change" }],
		openId: [{ required: true, message: "请输入应用标识", trigger: "blur" }],
		requestTimeout: [{ required: true, message: "请输入请求超时时间", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "应用OpenId",
});

const handleTemplateIdAdd = () => {
	state.formData.templateIdList.push({});
};

const handleTemplateIdDelete = (index: number) => {
	state.formData.templateIdList.splice(index, 1);
};

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
				await applicationOpenIdApi.addApplicationOpenId(formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await applicationOpenIdApi.editApplicationOpenId(formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await applicationOpenIdApi.queryApplicationOpenIdDetail(recordId);
		state.formData = { ...apiRes, loginComponent: apiRes.loginComponent ?? "" };
		state.dialogTitle = `应用OpenId详情 - ${apiRes.appName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加应用OpenId";
		state.formDisabled = false;
		state.formData = {
			appType: AppEnvironmentEnum.Web,
			environmentType: EnvironmentTypeEnum.Production,
			requestTimeout: 60000,
			requestEncipher: true,
			loginComponent: "",
			templateIdList: [],
		};
	});
};

const edit = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await applicationOpenIdApi.queryApplicationOpenIdDetail(recordId);
		state.formData = { ...apiRes, loginComponent: apiRes.loginComponent ?? "" };
		state.dialogTitle = `编辑应用OpenId - ${apiRes.openId}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
