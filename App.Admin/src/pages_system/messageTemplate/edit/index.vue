<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<FaTenantSelect v-model="state.formData.tenantId" v-model:tenant-name="state.formData.tenantName" prop="tenantId" />
			<FaApplicationClientSelect v-model="state.formData.appKey" prop="appKey" />
			<wd-form-item title="模板类型" prop="templateType">
				<FaRadioGroup name="MessageTemplateTypeEnum" v-model="state.formData.templateType" />
			</wd-form-item>
			<wd-form-item title="模板Code" prop="templateCode">
				<wd-textarea
					custom-style="--fa-textarea-height: 50px"
					v-model="state.formData.templateCode"
					clearable
					:maxlength="50"
					placeholder="请输入模板Code"
				/>
			</wd-form-item>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { MessageTemplateTypeEnum } from "@/api/enums/MessageTemplateTypeEnum";
import { messageTemplateApi } from "@/api/services/Center/messageTemplate";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddMessageTemplateInput } from "@/api/services/Center/messageTemplate/models/AddMessageTemplateInput";
import type { EditMessageTemplateInput } from "@/api/services/Center/messageTemplate/models/EditMessageTemplateInput";

definePage({
	name: "SystemMessageTemplateEdit",
	authCode: ["MessageTemplate:Add", "MessageTemplate:Edit"],
	style: {
		navigationBarTitleText: "消息模板",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<AddMessageTemplateInput & EditMessageTemplateInput>({ templateType: MessageTemplateTypeEnum.RegisterVerifyCode }),
	formSchema: useFormSchema<AddMessageTemplateInput & EditMessageTemplateInput>({
		templateType: { required: true, message: "请选择模板类型" },
		templateCode: { required: true, message: "请输入模板Code" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	recordId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
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
		state.loading = false;
	}
	pagingStore.incrementPageVersion("MessageTemplate");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/messageTemplate/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				uni.setNavigationBarTitle({ title: "添加消息模板" });
				break;
			case "edit":
				{
					const apiRes = await messageTemplateApi.queryMessageTemplateDetail(state.recordId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑消息模板 - ${apiRes.templateCode}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; recordId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.recordId)) {
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
