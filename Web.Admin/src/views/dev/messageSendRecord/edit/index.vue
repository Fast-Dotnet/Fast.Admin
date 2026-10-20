<template>
	<FaDialog ref="faDialogRef" width="1000" :title="state.dialogTitle" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form cols="2">
			<FaFormItem prop="channel" label="渠道">
				<Text name="MessageSendChannelEnum" :value="state.formData.channel" />
			</FaFormItem>
			<FaFormItem prop="isSuccess" label="状态">
				<el-tag style="width: auto" :type="state.formData.isSuccess ? 'success' : 'danger'">
					{{ state.formData.isSuccess ? "成功" : "失败" }}
				</el-tag>
			</FaFormItem>
			<FaFormItem prop="receiver" label="收件人">
				<el-text type="primary" v-iconCopy="state.formData.receiver">
					{{ state.formData.receiver }}
				</el-text>
			</FaFormItem>
			<FaFormItem prop="createdTime" label="发送时间">
				{{ dayjs(state.formData.createdTime).format("YYYY-MM-DD HH:mm:ss") }}
			</FaFormItem>
			<FaLayoutGridItem span="2">
				<FaFormItem prop="title" label="标题">
					{{ state.formData.title || "-" }}
				</FaFormItem>
			</FaLayoutGridItem>
		</FaForm>
		<el-divider content-position="left">内容</el-divider>
		<iframe
			v-if="state.formData.channel === MessageSendChannelEnum.Email"
			class="message-send-record-detail__email"
			:srcdoc="state.formData.recordValue"
			:sandbox="''"
		/>
		<VueJsonPretty
			v-else-if="jsonContent !== undefined"
			:data="jsonContent"
			:deep="3"
			show-length
			show-line-number
			show-icon
			:theme="configStore.layout.isDark ? 'dark' : 'light'"
		/>
		<pre v-else class="message-send-record-detail__text">
					{{ state.formData.recordValue || "-" }}
				</pre>
	</FaDialog>
</template>

<script lang="ts" setup>
import { computed, reactive, useTemplateRef } from "vue";
import { dayjs } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import VueJsonPretty from "vue-json-pretty";
import { MessageSendChannelEnum } from "@/api/enums/MessageSendChannelEnum";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";
import { useConfig } from "@/stores";
import type { FaDialogInstance } from "fast-element-plus";
import type { JSONDataType } from "vue-json-pretty/types/utils";
import type { QueryMessageSendRecordDetailOutput } from "@/api/services/Center/messageSendRecord/models/QueryMessageSendRecordDetailOutput";

defineOptions({
	name: "DevMessageSendRecordEdit",
});

const configStore = useConfig();
const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryMessageSendRecordDetailOutput>({}),
	dialogTitle: "消息记录",
});

const jsonContent = computed<JSONDataType | undefined>(() => {
	if (!state.formData.recordValue) return undefined;
	try {
		return JSON.parse(state.formData.recordValue) as JSONDataType;
	} catch {
		return undefined;
	}
});

const detail = (recordId?: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await messageSendRecordApi.queryMessageSendRecordDetail(recordId);
		state.formData = apiRes;
		state.dialogTitle = `消息记录详情 - ${apiRes.receiver}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>

<style lang="scss" scoped>
.message-send-record-detail__email {
	width: 100%;
	height: 500px;
	border: var(--el-border);
	border-radius: var(--el-border-radius-base);
	background: #fff;
}

.message-send-record-detail__text {
	overflow: auto;
	max-height: 500px;
	margin: 0;
	padding: 16px;
	white-space: pre-wrap;
	overflow-wrap: anywhere;
	border: var(--el-border);
	border-radius: var(--el-border-radius-base);
}
</style>
