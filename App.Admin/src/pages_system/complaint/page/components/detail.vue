<template>
	<FaPopup ref="popupRef" title="投诉工单详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="应用名称" :value="state.formData.appName" />
			<FaField label="应用标识" :value="state.formData.appKey" />
			<FaField label="投诉类型">
				<FaTag name="ComplaintTypeEnum" :value="state.formData.complaintType" />
			</FaField>
			<FaField label="昵称" :value="state.formData.nickName" />
			<FaField label="手机" :value="state.formData.mobile" />
			<FaField label="联系电话" :value="state.formData.contactPhone" />
			<FaField label="联系邮箱" :value="state.formData.contactEmail" />
			<FaField label="投诉描述" :value="state.formData.description" selectable />
			<FaField label="附件图片">
				<FaImage v-for="(item, index) in state.formData.attachmentImages" :key="index" width="96rpx" height="96rpx" :src="item" />
			</FaField>
			<FaField label="处理时间" :value="state.formData.handleTime" />
			<FaField label="处理描述" :value="state.formData.handleDescription" selectable />
			<FaField label="备注" :value="state.formData.remark" />
			<FaField label="租户名称" :value="state.formData.tenantName" />
			<FaField label="创建时间" :value="state.formData.createdTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
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

const state = reactive({
	formData: withDefineType<QueryComplaintPagedOutput>({}),
});

const detail = (data: QueryComplaintPagedOutput) => {
	popupRef.value?.open(() => {
		state.formData = data;
	});
};

defineExpose({
	detail,
});
</script>
