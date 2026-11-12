<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" :disabled="state.formDisabled" border>
			<template v-if="state.pageMode === 'detail'">
				<wd-form-item title="租户">
					<text v-if="state.formData.tenantName">{{ state.formData.tenantName }}</text>
					<text v-else style="color: var(--wot-text-placeholder)">未指定租户</text>
				</wd-form-item>
				<wd-form-item title="应用标识">
					<text v-if="state.formData.appKey">{{ state.formData.appKey }}</text>
					<text v-else style="color: var(--wot-text-placeholder)">{{ state.formData.tenantName ? "不区分应用" : "全局模板" }}</text>
				</wd-form-item>
			</template>
			<template v-else>
				<FaTenantSelect v-model="state.formData.tenantId" v-model:tenant-name="state.formData.tenantName" />
				<FaApplicationClientSelect v-model="state.formData.appKey" />
			</template>

			<wd-form-item title="模板类型" prop="templateType">
				<FaRadioGroup v-model="state.formData.templateType" name="MessageTemplateTypeEnum" />
			</wd-form-item>
			<wd-form-item title="模板Code" prop="templateCode">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 100rpx"
					v-model="state.formData.templateCode"
					clearable
					:maxlength="50"
					placeholder="请输入模板Code"
				/>
			</wd-form-item>

			<template v-if="state.pageMode === 'detail'">
				<wd-form-item title="创建人" prop="createdUserName">
					{{ state.formData.createdUserName }}
				</wd-form-item>
				<wd-form-item title="创建时间" prop="createdTime">
					{{ state.formData.createdTime }}
				</wd-form-item>
				<wd-form-item title="更新人" prop="updatedUserName">
					{{ state.formData.updatedUserName }}
				</wd-form-item>
				<wd-form-item title="更新时间" prop="updatedTime">
					{{ state.formData.updatedTime }}
				</wd-form-item>
			</template>
		</wd-form>

		<wd-button v-if="!state.formDisabled" custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";
import { messageTemplateApi } from "@/api/services/Center/messageTemplate";
import { useFormSchema, useLoading, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddMessageTemplateInput } from "@/api/services/Center/messageTemplate/models/AddMessageTemplateInput";
import type { QueryMessageTemplateDetailOutput } from "@/api/services/Center/messageTemplate/models/QueryMessageTemplateDetailOutput";

definePage({
	name: "SystemMessageTemplateEdit",
	authCode: ["MessageTemplate:Detail", "MessageTemplate:Add", "MessageTemplate:Edit"],
	style: {
		navigationBarTitleText: "消息模板",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<QueryMessageTemplateDetailOutput>({ templateType: MessageTemplateTypeEnum.RegisterVerifyCode }),
	formSchema: useFormSchema<AddMessageTemplateInput>({
		templateType: { required: true, message: "请选择模板类型" },
		templateCode: { required: true, message: "请输入模板Code" },
	}),
	formDisabled: true,
	pageMode: withDefineType<IPageStateType>("detail"),
	recordId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
	try {
		switch (state.pageMode) {
			case "add":
				await messageTemplateApi.addMessageTemplate(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await messageTemplateApi.editMessageTemplate(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		useLoading.hide();
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/messageTemplate/page/index");
});

const handleLoad = async () => {
	switch (state.pageMode) {
		case "detail":
			{
				state.formDisabled = true;
				const apiRes = await messageTemplateApi.queryMessageTemplateDetail(state.recordId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `消息模板详情 - ${apiRes.templateCode}` });
			}
			break;
		case "add":
			state.formDisabled = false;
			uni.setNavigationBarTitle({ title: "添加消息模板" });
			break;
		case "edit":
			{
				state.formDisabled = false;
				const apiRes = await messageTemplateApi.queryMessageTemplateDetail(state.recordId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `编辑消息模板 - ${apiRes.templateCode}` });
			}
			break;
	}
};

onLoad((options: { mode?: IPageStateType; recordId?: string }) => {
	if (!options?.mode || ((options.mode === "edit" || options.mode === "detail") && !options?.recordId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.recordId = options.recordId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
