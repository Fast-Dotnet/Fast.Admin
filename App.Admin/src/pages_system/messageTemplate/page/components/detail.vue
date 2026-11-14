<template>
	<FaPopup ref="popupRef" title="消息模板详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="模板类型">
				<FaTag name="MessageTemplateTypeEnum" :value="formData.templateType" />
			</FaField>
			<FaField label="模板Code" :value="formData.templateCode" copyable />
			<FaField label="租户" :value="formData.tenantName || '未指定租户'" />
			<FaField label="应用标识" :value="formData.appKey || (formData.tenantName ? '不区分应用' : '全局模板')" :selectable="!!formData.appKey" />
			<FaField label="应用名称" :value="formData.appName" />
			<FaField label="应用类型">
				<FaTagFlag name="AppEnvironmentEnum" :value="formData.appType" />
			</FaField>
			<FaField label="环境类型">
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
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryMessageTemplateDetailOutput>();

const detail = throttle((recordId: string) =>
	popupRef.value.open(async () => {
		formData.value = await messageTemplateApi.queryMessageTemplateDetail(recordId);
	})
);

defineExpose({
	detail,
});
</script>
