<template>
	<FaPopup ref="faPopupRef" title="配置详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="配置名称" :value="formData.configName" selectable />
			<FaField label="配置编码" :value="formData.configCode" copyable />
			<FaField label="配置值" :value="formData.configValue" color="var(--wot-primary-6)" selectable />
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
import { configApi } from "@/api/services/Center/config";
import type { QueryConfigDetailOutput } from "@/api/services/Center/config/models/QueryConfigDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevConfigDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryConfigDetailOutput>({});

const detail = throttle(
	async (configId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await configApi.queryConfigDetail(configId);
		})
);

defineExpose({
	detail,
});
</script>
