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
					<FaCellPicker title="成功" v-model="searchParam.value.isSuccess" name="BooleanEnum" radio clearable />
					<FaCellPicker title="渠道" v-model="searchParam.value.channel" name="MessageSendChannelEnum" clearable />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.recordId"
				:title="item.receiver"
				code="MessageSendRecord:Detail"
				@click="detailFormRef?.detail(item.recordId)"
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
				<FaField label="发送人" :value="item.createdUserName" />
				<FaField label="发送时间" :value="item.createdTime" />
				<FaField label="地区" :value="`${item.province ?? ''} - ${item.city ?? ''}`" />
				<FaField label="Ip" :value="item.ip" />
				<FaField label="设备" :value="item.device" />
				<FaField label="操作系统" :value="item.os" />
				<FaField label="浏览器" :value="item.browser" />
			</FaCard>
		</template>
	</FaPaging>

	<MessageSendRecordDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";
import MessageSendRecordDetail from "./components/detail.vue";

definePage({
	name: "DevMessageSendRecord",
	authCode: "MessageSendRecord:Paged",
	style: {
		navigationBarTitleText: "消息记录",
	},
});

const detailFormRef = ref<InstanceType<typeof MessageSendRecordDetail>>();
</script>
