<template>
	<FaPaging :request-api="exceptionLogApi.queryExceptionLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput
				data-search-range="Past3M"
				v-model:search-param="searchParam.value"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			>
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaTenantSelect v-model="searchParam.value.tenantId" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="detailFormRef?.detail(item)">
				<FaField label="账号" :value="[item.nickName, item.mobile].filter(Boolean).join(' · ')" />
				<FaField label="异常信息" :value="item.message" color="var(--wot-danger-main)" />
				<FaField label="类名" :value="item.className" />
				<FaField label="方法名" :value="item.methodName" />
				<FaField label="异常源" :value="item.source" />
				<FaField label="租户" :value="item.tenantName" />
			</FaCard>
		</template>
	</FaPaging>

	<ExceptionLogDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { exceptionLogApi } from "@/api/services/Center/exceptionLog";
import ExceptionLogDetail from "./components/detail.vue";

definePage({
	name: "DevExceptionLog",
	authCode: "ExceptionLog:Paged",
	style: {
		navigationBarTitleText: "异常日志",
	},
});

const detailFormRef = ref<InstanceType<typeof ExceptionLogDetail>>();
</script>
