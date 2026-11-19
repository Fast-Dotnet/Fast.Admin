<template>
	<FaPopup ref="faPopupRef" title="应用版本详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="应用名称" :value="formData.appName" selectable />
			<FaField label="应用标识" :value="formData.appKey" copyable />
			<FaField label="应用类型">
				<FaTagFlag name="AppEnvironmentEnum" :value="formData.appType" />
			</FaField>
			<FaField label="运行环境">
				<FaTag name="EnvironmentTypeEnum" :value="formData.environmentType" />
			</FaField>
			<FaField label="包类型">
				<FaTag name="ApplicationPackageTypeEnum" :value="formData.packageType" />
			</FaField>
			<FaField label="版本名称" :value="formData.versionName" selectable />
			<FaField label="版本号" :value="formData.versionCode" selectable />
			<FaField label="状态">
				<FaTag name="CommonStatusEnum" :value="formData.status" />
			</FaField>
			<template v-if="formData.packageType === ApplicationPackageTypeEnum.Wgt">
				<FaField label="最低版本" :value="formData.minNativeVersionCode" selectable />
				<FaField label="最高版本" :value="formData.maxNativeVersionCode" selectable />
			</template>
			<FaField label="强制更新">
				<FaTag name="BooleanEnum" :value="formData.isForceUpdate" />
			</FaField>
			<FaField label="应用包" :value="formData.fileUrl" copyable />
			<FaField label="更新说明" :value="formData.updateContent" selectable />
			<FaField label="备注" :value="formData.remark" selectable />
			<FaField label="创建人" :value="formData.createdUserName" selectable />
			<FaField label="创建时间" :value="formData.createdTime" selectable />
			<FaField label="更新人" :value="formData.updatedUserName" selectable />
			<FaField label="更新时间" :value="formData.updatedTime" selectable />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import { applicationVersionApi } from "@/api/services/Center/applicationVersion";
import type { QueryApplicationVersionDetailOutput } from "@/api/services/Center/applicationVersion/models/QueryApplicationVersionDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemApplicationVersionDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryApplicationVersionDetailOutput>({});

const detail = throttle(
	async (recordId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await applicationVersionApi.queryApplicationVersionDetail(recordId);
		})
);

defineExpose({
	detail,
});
</script>
