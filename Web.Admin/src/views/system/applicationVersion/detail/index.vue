<template>
	<FaDialog ref="faDialogRef" width="800" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form cols="2">
			<FaFormItem label="应用名称" :value="state.formData.appName" />
			<FaFormItem label="应用标识" :value="state.formData.appKey" />
			<FaFormItem label="应用类型">
				<TagFlag name="AppEnvironmentEnum" :value="state.formData.appType" />
			</FaFormItem>
			<FaFormItem label="环境类型">
				<Tag name="EnvironmentTypeEnum" :value="state.formData.environmentType" />
			</FaFormItem>
			<FaFormItem label="包类型">
				<Tag name="ApplicationPackageTypeEnum" :value="state.formData.packageType" />
			</FaFormItem>
			<FaFormItem label="状态">
				<Tag name="CommonStatusEnum" :value="state.formData.status" />
			</FaFormItem>
			<FaFormItem label="版本名称" :value="state.formData.versionName" />
			<FaFormItem label="版本号" :value="state.formData.versionCode" />
			<template v-if="state.formData.packageType === ApplicationPackageTypeEnum.Wgt">
				<FaFormItem label="最低版本" :value="state.formData.minNativeVersionCode" />
				<FaFormItem label="最高版本" :value="state.formData.maxNativeVersionCode" />
			</template>
			<FaFormItem label="强制更新" span="2">
				<Tag name="BooleanEnum" :value="state.formData.isForceUpdate" />
			</FaFormItem>
			<FaFormItem label="应用包" :value="state.formData.fileUrl" span="2" />
			<FaFormItem label="更新说明" :value="state.formData.updateContent" span="2" />
			<FaFormItem label="备注" :value="state.formData.remark" span="2" />
			<FaFormItem label="创建人" :value="state.formData.createdUserName" />
			<FaFormItem label="创建时间" :value="state.formData.createdTime" />
			<FaFormItem label="更新人" :value="state.formData.updatedUserName" />
			<FaFormItem label="更新时间" :value="state.formData.updatedTime" />
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import { applicationVersionApi } from "@/api/services/Center/applicationVersion";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryApplicationVersionDetailOutput } from "@/api/services/Center/applicationVersion/models/QueryApplicationVersionDetailOutput";

defineOptions({
	name: "SystemApplicationVersionDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryApplicationVersionDetailOutput>({}),
	dialogTitle: "应用版本",
});

const detail = (recordId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await applicationVersionApi.queryApplicationVersionDetail(recordId);
		state.formData = apiRes;
		state.dialogTitle = `应用版本详情 - ${apiRes.versionName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>

<style scoped lang="scss">
.application-version-detail__text {
	min-width: 0;
	max-width: 100%;
	white-space: pre-wrap;
	overflow-wrap: anywhere;
}
</style>
