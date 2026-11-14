<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<FaApplicationClientSelect v-model="state.formData.appKey" prop="appKey" />
			<wd-form-item title="凭据类型" prop="tokenType">
				<FaRadioGroup name="ApplicationTokenTypeEnum" v-model="state.formData.tokenType" type="button" />
			</wd-form-item>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { ApplicationTokenTypeEnum } from "@/api/enums/ApplicationTokenTypeEnum";
import { applicationTokenApi } from "@/api/services/Center/applicationToken";
import { useFormSchema, useLoading, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddApplicationTokenInput } from "@/api/services/Center/applicationToken/models/AddApplicationTokenInput";

definePage({
	name: "SystemApplicationTokenAdd",
	authCode: "AppToken:Add",
	style: {
		navigationBarTitleText: "添加应用凭据",
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<AddApplicationTokenInput>({ tokenType: ApplicationTokenTypeEnum.AccessToken }),
	formSchema: useFormSchema<AddApplicationTokenInput>({
		appKey: { required: true, message: "请选择应用标识" },
		tokenType: { required: true, message: "请选择凭据类型" },
	}),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
	try {
		await applicationTokenApi.addApplicationToken(state.formData);
		useToast.success("新增成功！");
	} finally {
		useLoading.hide();
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/applicationToken/page/index");
});
</script>
