<template>
	<FaPopup ref="faPopupRef" title="消息模板详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="模板类型" center>
				<FaTag name="MessageTemplateTypeEnum" :value="formData.templateType" />
			</FaField>
			<FaField label="模板Code" :value="formData.templateCode" copyable />
			<FaField label="租户">
				<text v-if="formData.tenantName">{{ formData.tenantName }}</text>
				<text v-else style="color: var(--wot-text-placeholder)">未指定租户</text>
			</FaField>
			<FaField label="应用标识">
				<text v-if="formData.appKey">{{ formData.appKey }}</text>
				<text v-else style="color: var(--wot-text-placeholder)">{{ formData.tenantName ? "不区分应用" : "全局模板" }}</text>
			</FaField>
			<FaField label="应用名称" :value="formData.appName" />
			<FaField label="应用类型">
				<FaTagFlag name="AppEnvironmentEnum" :value="formData.appType" />
			</FaField>
			<FaField label="环境类型" center>
				<FaTag name="EnvironmentTypeEnum" :value="formData.environmentType" />
			</FaField>
			<FaField label="创建人" :value="formData.createdUserName" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新人" :value="formData.updatedUserName" />
			<FaField label="更新时间" :value="formData.updatedTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { messageTemplateApi } from "@/api/services/Center/messageTemplate";
import type { QueryMessageTemplateDetailOutput } from "@/api/services/Center/messageTemplate/models/QueryMessageTemplateDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemMessageTemplateDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryMessageTemplateDetailOutput>();

const detail = throttle(
	async (recordId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await messageTemplateApi.queryMessageTemplateDetail(recordId);
		})
);

defineExpose({
	detail,
});
</script>
