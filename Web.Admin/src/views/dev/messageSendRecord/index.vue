<template>
	<div>
		<FaTable
			row-key="recordId"
			:request-api="messageSendRecordApi.queryMessageSendRecordPaged"
			:columns="searchColumns"
			data-search-range="Past3M"
		>
			<FaTableColumn
				prop="channel"
				label="渠道"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('MessageSendChannelEnum')"
			/>
			<FaTableColumn prop="receiver" label="收件人" width="200" small-width="180" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="title" label="标题" width="300" small-width="280" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="isSuccess" label="成功" width="100" small-width="80" sortable tag :enum="appStore.getDictionary('BooleanEnum')" />
			<FaTableColumn
				prop="createdTime"
				label="发送时间"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryMessageSendRecordPagedOutput }">
					<span>地区：{{ row.province }} - {{ row.city }}</span>
					<br />
					<span>Ip：{{ row.ip }}</span>
					<br />
					<span>时间：{{ dayjs(row.createdTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					<el-tag v-if="row.createdTime" type="info" round effect="light" size="small" style="margin-left: 5px !important">
						{{ formatChineseRelativeTime(row.createdTime) }}
					</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="os" label="发送设备" width="280" small-width="260" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: QueryMessageSendRecordPagedOutput }">
					<span>设备：{{ row.device }}</span>
					<br />
					<span>操作系统：{{ row.os }}</span>
					<br />
					<span>浏览器：{{ row.browser }}</span>
				</template>
			</FaTableColumn>

			<template #operation="{ row }: { row?: QueryMessageSendRecordPagedOutput }">
				<el-button v-auth="'MessageSendRecord:Detail'" size="small" plain @click="editFormRef.detail(row.recordId)">详情</el-button>
			</template>
		</FaTable>
		<MessageSendRecordEdit ref="editFormRef" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";
import { useApp } from "@/stores";
import MessageSendRecordEdit from "./edit/index.vue";
import type { FaTableColumnCtx } from "fast-element-plus";
import type { QueryMessageSendRecordPagedOutput } from "@/api/services/Center/messageSendRecord/models/QueryMessageSendRecordPagedOutput";

defineOptions({
	name: "DevMessageSendRecord",
});

const appStore = useApp();

const editFormRef = useTemplateRef<InstanceType<typeof MessageSendRecordEdit>>("editFormRef");

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "isSuccess",
		label: "成功",
		enum: appStore.getDictionary("BooleanEnum"),
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "channel",
		label: "渠道",
		enum: appStore.getDictionary("MessageSendChannelEnum"),
		search: { el: "el-select" },
	},
];
</script>
