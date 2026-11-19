<template>
	<FaPaging page-key="SqlTimeoutLog" :request-api="sqlTimeoutLogApi.querySqlTimeoutLogPaged">
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
				<FaField
					label="超时秒数"
					:value="item.timeoutSeconds == null ? '--' : `${item.timeoutSeconds.toFixed(4)}秒`"
					color="var(--wot-text-auxiliary)"
				/>
				<FaField label="方法名" :value="item.methodName" />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="地区" :value="[item.province, item.city].filter(Boolean).join(' - ')" />
				<FaField label="Ip" :value="item.ip" copyable />
				<FaField label="设备" :value="[item.device, item.os].filter(Boolean).join(' · ')" />
				<FaField label="浏览器" :value="item.browser" />
				<FaField label="租户" :value="item.tenantName" />
			</FaCard>
		</template>
	</FaPaging>

	<SqlTimeoutLogDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { sqlTimeoutLogApi } from "@/api/services/Center/sqlTimeoutLog";
import SqlTimeoutLogDetail from "./components/detail.vue";

definePage({
	name: "DevSqlTimeoutLog",
	authCode: "SqlTimeoutLog:Paged",
	style: {
		navigationBarTitleText: "Sql超时日志",
	},
});

const detailFormRef = ref<InstanceType<typeof SqlTimeoutLogDetail>>();
</script>
