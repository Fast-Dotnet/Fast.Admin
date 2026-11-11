<template>
	<FaPopup ref="popupRef" title="消息详情" height="70%" position="bottom" closable>
		<scroll-view scroll-y>
			<view style="display: flex; flex-direction: column; gap: 10px">
				<view class="log__title">{{ state.formData.receiver }}</view>
				<FaField label="渠道">
					<FaTag name="MessageSendChannelEnum" :value="state.formData.channel" />
				</FaField>
				<FaField label="标题" :value="state.formData.title" />
				<FaField label="成功">
					<FaTag name="BooleanEnum" :value="state.formData.isSuccess" />
				</FaField>
				<FaField label="发送时间" :value="state.formData.createdTime" />
				<FaField label="地区" :value="`${state.formData.province} - ${state.formData.city}`" />
				<FaField label="Ip" :value="state.formData.ip" />
				<FaField label="设备" :value="state.formData.device" />
				<FaField label="操作系统" :value="state.formData.os" />
				<FaField label="浏览器" :value="state.formData.browser" />

				<view class="log__title">消息内容</view>
				<rich-text :nodes="state.formData.recordValue" />
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";
import type { QueryMessageSendRecordDetailOutput } from "@/api/services/Center/messageSendRecord/models/QueryMessageSendRecordDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevMessageSendRecordDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<QueryMessageSendRecordDetailOutput>({}),
});

const detail = throttle(async (recordId?: string) => {
	if (!recordId) return;
	await popupRef.value?.open(async () => {
		state.formData = await messageSendRecordApi.queryMessageSendRecordDetail(recordId);
	});
});

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log__title {
	font-size: var(--wot-typography-body-size-large);
	font-weight: var(--wot-font-weight-semibold);
	overflow-wrap: anywhere;
}
</style>
