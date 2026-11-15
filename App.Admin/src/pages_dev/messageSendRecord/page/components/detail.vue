<template>
	<FaPopup ref="popupRef" title="消息详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="收件人" :value="state.formData.receiver" />
			<FaField label="渠道">
				<FaText name="MessageSendChannelEnum" :value="state.formData.channel" />
			</FaField>
			<FaField label="标题" :value="state.formData.title" />
			<FaField label="发送结果">
				<FaTag name="BooleanEnum" :value="state.formData.isSuccess" />
			</FaField>
			<FaField label="发送人" :value="state.formData.createdUserName" />
			<FaField label="发送时间" :value="state.formData.createdTime" />
			<FaField label="省份" :value="state.formData.province" />
			<FaField label="城市" :value="state.formData.city" />
			<FaField label="Ip" :value="state.formData.ip" />
			<FaField label="设备" :value="state.formData.device" />
			<FaField label="操作系统" :value="state.formData.os" />
			<FaField label="浏览器" :value="state.formData.browser" />

			<view class="log-content__title">消息内容</view>
			<text v-if="state.formData.channel === MessageSendChannelEnum.Email" class="log-content__empty">暂不支持邮件预览</text>
			<text v-else-if="state.contentText" class="log-content__text" selectable user-select>{{ state.contentText }}</text>
			<text v-else class="log-content__empty">暂无内容</text>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { MessageSendChannelEnum } from "@/api/enums/MessageSendChannelEnum";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";
import type { QueryMessageSendRecordDetailOutput } from "@/api/services/Center/messageSendRecord/models/QueryMessageSendRecordDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevMessageSendRecordDetail",
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<QueryMessageSendRecordDetailOutput>({}),
	contentText: "",
});

const detail = throttle(async (recordId: string) => {
	await popupRef.value.open(async () => {
		const apiRes = await messageSendRecordApi.queryMessageSendRecordDetail(recordId);
		state.formData = apiRes;
		if (apiRes.channel !== MessageSendChannelEnum.Email) {
			if (apiRes.recordValue) {
				try {
					state.contentText = JSON.stringify(JSON.parse(apiRes.recordValue), null, 2);
				} catch {
					// 参数不是合法 JSON 时保留原文。
				}
			}
		}
	});
});

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log-content__title {
	font-weight: var(--wot-font-weight-semibold);
}

.log-content__text {
	display: block;
	padding: 12px;
	border: 1px solid var(--wot-border-light);
	border-radius: 8px;
	background: var(--wot-filled-oppo);
	font-family: monospace;
	font-size: var(--wot-typography-label-size-main);
	line-height: 1.6;
	white-space: pre-wrap;
	overflow-wrap: anywhere;
}

.log-content__empty {
	color: var(--wot-text-auxiliary);
}
</style>
