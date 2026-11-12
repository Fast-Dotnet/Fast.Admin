<template>
	<FaPaging :request-api="messageSendRecordApi.queryMessageSendRecordPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput
				data-search-range="Past3M"
				v-model:search-param="searchParam.value"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			>
				<template #filter>
					<FaCellPicker title="成功" v-model="searchParam.value.isSuccess" name="BooleanEnum" radio />
					<FaCellPicker title="渠道" v-model="searchParam.value.channel" name="MessageSendChannelEnum" placeholder="请选择" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.recordId"
				:title="item.receiver"
				code="MessageSendRecord:Detail"
				@click="editFormRef.detail(item.recordId)"
			>
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="MessageSendChannelEnum" :value="item.channel" />
					</view>
				</template>

				<FaField label="标题" :value="item.title" />
				<FaField label="成功">
					<FaTag name="BooleanEnum" :value="item.isSuccess" />
				</FaField>
				<FaField label="发送时间" :value="item.createdTime" />
			</FaCard>
		</template>
	</FaPaging>

	<MessageSendRecordDetail ref="editFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";
import MessageSendRecordDetail from "./detail/index.vue";

definePage({
	name: "DevMessageSendRecord",
	authCode: "MessageSendRecord:Paged",
	style: {
		navigationBarTitleText: "消息记录",
	},
});

const editFormRef = ref<InstanceType<typeof MessageSendRecordDetail>>();
</script>
