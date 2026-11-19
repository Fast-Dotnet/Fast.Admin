<template>
	<FaPopup ref="faPopupRef" title="投诉工单详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="应用名称" :value="formData.appName" selectable />
			<FaField label="应用标识" :value="formData.appKey" copyable />
			<FaField label="投诉类型" center>
				<FaTag name="ComplaintTypeEnum" :value="formData.complaintType" />
			</FaField>
			<FaField label="昵称" :value="formData.nickName" selectable />
			<FaField label="手机" :value="formData.mobile" copyable />
			<FaField label="联系电话" :value="formData.contactPhone" copyable />
			<FaField label="联系邮箱" :value="formData.contactEmail" copyable />
			<FaField label="投诉描述" :value="formData.description" selectable />
			<FaField label="附件图片">
				<FaImage v-for="(item, index) in formData.attachmentImages" :key="index" width="96rpx" height="96rpx" :src="item" />
			</FaField>
			<FaField label="租户名称" :value="formData.tenantName" selectable />
			<FaField label="创建时间" :value="formData.createdTime" selectable />

			<wd-divider content-position="left">处理信息</wd-divider>
			<FaField label="处理状态" center>
				<wd-tag v-if="formData.handleTime" type="success">已处理</wd-tag>
				<wd-tag v-else type="warning">待处理</wd-tag>
			</FaField>
			<FaField label="处理时间" :value="formData.handleTime" selectable />
			<FaField label="处理描述" :value="formData.handleDescription" selectable />
			<FaField label="备注" :value="formData.remark" selectable />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import type { QueryComplaintPagedOutput } from "@/api/services/Center/complaint/models/QueryComplaintPagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemComplaintDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryComplaintPagedOutput>({});

const detail = throttle(
	async (data: QueryComplaintPagedOutput) =>
		await faPopupRef.value.open(() => {
			formData.value = data;
		})
);

defineExpose({
	detail,
});
</script>
