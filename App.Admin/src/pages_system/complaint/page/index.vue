<template>
	<FaPaging ref="pagingRef" :request-api="complaintApi.queryComplaintPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="投诉类型" v-model="searchParam.value.complaintType" name="ComplaintTypeEnum" radio clearable />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.complaintId"
				:title="item.appName"
				code="Complaint:Detail"
				@click="detailFormRef?.detail(item)"
			>
				<FaField label="投诉类型">
					<FaTag name="ComplaintTypeEnum" :value="item.complaintType" />
				</FaField>
				<FaField label="昵称" :value="item.nickName" />
				<FaField label="手机" :value="item.mobile" />
				<FaField label="租户名称" :value="item.tenantName" />
				<FaField label="投诉描述" :value="item.description" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="处理状态">
					<wd-tag v-if="item.handleTime" type="success">已处理</wd-tag>
					<wd-tag v-else type="warning">待处理</wd-tag>
				</FaField>
				<FaField label="处理时间" :value="item.handleTime" />

				<template v-if="!item.handleTime" #footer>
					<FaAuth code="Complaint:Handle">
						<wd-button size="mini" type="primary" variant="plain" @click="handleFormRef?.open(item.complaintId)">处理</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<ComplaintDetail ref="detailFormRef" />
	<ComplaintHandle ref="handleFormRef" @ok="pagingRef.reload()" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { complaintApi } from "@/api/services/Center/complaint";
import ComplaintDetail from "./components/detail.vue";
import ComplaintHandle from "./components/handle.vue";
import type { FaPagingInstance } from "@/components/index.ts";

definePage({
	name: "SystemComplaint",
	authCode: "Complaint:Paged",
	style: {
		navigationBarTitleText: "投诉工单",
	},
});

const pagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof ComplaintDetail>>();
const handleFormRef = ref<InstanceType<typeof ComplaintHandle>>();
</script>
