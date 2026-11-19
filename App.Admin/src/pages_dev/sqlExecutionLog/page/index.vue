<template>
	<FaPaging page-key="SqlExecutionLog" :request-api="sqlExecutionLogApi.querySqlExecutionLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaTenantSelect v-model="searchParam.value.tenantId" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="detailFormRef.detail(item)">
				<FaField label="账号" :value="[item.nickName, item.mobile].filter(Boolean).join(' · ')" />
				<FaField
					label="执行秒数"
					:value="item.executeSeconds == null ? '--' : `${item.executeSeconds.toFixed(4)}秒`"
					color="var(--wot-text-auxiliary)"
				/>
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="地区" :value="[item.province, item.city].filter(Boolean).join(' - ')" />
				<FaField label="Ip" :value="item.ip" copyable />
				<FaField label="设备" :value="[item.device, item.os].filter(Boolean).join(' · ')" />
				<FaField label="浏览器" :value="item.browser" />
				<FaField label="租户" :value="item.tenantName" />
			</FaCard>
		</template>
	</FaPaging>

	<SqlExecutionLogDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { sqlExecutionLogApi } from "@/api/services/Center/sqlExecutionLog";
import SqlExecutionLogDetail from "./components/detail.vue";

definePage({
	name: "DevSqlExecutionLog",
	authCode: "SqlExecutionLog:Paged",
	style: {
		navigationBarTitleText: "Sql执行日志",
	},
});

const detailFormRef = ref<InstanceType<typeof SqlExecutionLogDetail>>();
</script>
