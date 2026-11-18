<template>
	<FaPaging page-key="SqlExceptionLog" :request-api="sqlExceptionLogApi.querySqlExceptionLogPaged">
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
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="detailFormRef.detail(item)">
				<FaField label="账号" :value="[item.nickName, item.mobile].filter(Boolean).join(' · ')" />
				<FaField label="异常信息" :value="item.message" color="var(--wot-danger-main)" />
				<FaField label="文件名称" :value="item.fileName" />
				<FaField label="文件行数" :value="item.fileLine" />
				<FaField label="方法名" :value="item.methodName" />
				<FaField label="租户" :value="item.tenantName" />
			</FaCard>
		</template>
	</FaPaging>

	<SqlExceptionLogDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { sqlExceptionLogApi } from "@/api/services/Center/sqlExceptionLog";
import SqlExceptionLogDetail from "./components/detail.vue";

definePage({
	name: "DevSqlExceptionLog",
	authCode: "SqlExceptionLog:Paged",
	style: {
		navigationBarTitleText: "Sql异常日志",
	},
});

const detailFormRef = ref<InstanceType<typeof SqlExceptionLogDetail>>();
</script>
