<template>
	<FaPopup ref="popupRef" title="投诉工单详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="应用名称" :value="formData.appName" />
			<FaField label="应用标识" :value="formData.appKey" />
			<FaField label="投诉类型">
				<FaTag name="ComplaintTypeEnum" :value="formData.complaintType" />
			</FaField>
			<FaField label="昵称" :value="formData.nickName" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="联系电话" :value="formData.contactPhone" />
			<FaField label="联系邮箱" :value="formData.contactEmail" />
			<FaField label="投诉描述" :value="formData.description" selectable />
			<FaField label="附件图片">
				<FaImage v-for="(item, index) in formData.attachmentImages" :key="index" width="96rpx" height="96rpx" :src="item" />
			</FaField>
			<FaField label="处理时间" :value="formData.handleTime" />
			<FaField label="处理描述" :value="formData.handleDescription" selectable />
			<FaField label="备注" :value="formData.remark" />
			<FaField label="租户名称" :value="formData.tenantName" />
			<FaField label="创建时间" :value="formData.createdTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import type { QueryComplaintPagedOutput } from "@/api/services/Center/complaint/models/QueryComplaintPagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemComplaintDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryComplaintPagedOutput>();

const detail = (data: QueryComplaintPagedOutput) => {
	popupRef.value.open(() => {
		formData.value = data;
	});
};

defineExpose({
	detail,
});
</script>
