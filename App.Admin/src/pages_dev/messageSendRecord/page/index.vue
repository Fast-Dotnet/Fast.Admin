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
				:title="item.title"
				code="MessageSendRecord:Detail"
				@click="handleDetail(item.recordId)"
			>
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="MessageSendChannelEnum" :value="item.channel" />
					</view>
				</template>

				<FaField label="收件人" :value="item.receiver" />
				<FaField label="发送结果">
					<FaTag name="BooleanEnum" :value="item.isSuccess" />
				</FaField>
				<FaField label="发送人" :value="item.createdUserName" />
				<FaField label="发送时间" :value="item.createdTime" />
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";

definePage({
	name: "DevMessageSendRecord",
	authCode: "MessageSendRecord:Paged",
	style: {
		navigationBarTitleText: "消息记录",
	},
});

const router = useRouter();

const handleDetail = throttle((recordId: string) => router.push(`/pages_dev/messageSendRecord/detail/index?recordId=${recordId}`));
</script>
