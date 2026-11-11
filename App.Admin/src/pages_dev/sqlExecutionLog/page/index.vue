<template>
	<FaPaging :request-api="sqlExecutionLogApi.querySqlExecutionLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaTenantSelect v-model="searchParam.value.tenantId" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="editFormRef?.detail(item)">
				<FaField label="昵称" :value="`${item.nickName} · ${item.mobile}`" />
				<FaField
					label="秒数"
					:value="item.executeSeconds == null ? '--' : `${item.executeSeconds.toFixed(4)}秒`"
					color="var(--wot-text-auxiliary)"
				/>
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="地区" :value="`${item.province} - ${item.city}`" />
				<FaField label="Ip" :value="item.ip" />
				<FaField label="设备" :value="item.device" />
				<FaField label="操作系统" :value="item.os" />
				<FaField label="浏览器" :value="item.browser" />
				<FaField label="租户" :value="item.tenantName" />
			</FaCard>
		</template>
	</FaPaging>

	<SqlExecutionLogDetail ref="editFormRef" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import { sqlExecutionLogApi } from "@/api/services/Center/sqlExecutionLog";
import SqlExecutionLogDetail from "./detail/index.vue";

definePage({
	name: "DevSqlExecutionLog",
	layout: "layout",
	authCode: "SqlExecutionLog:Paged",
	style: {
		navigationBarTitleText: "Sql执行日志",
	},
});

const editFormRef = ref<InstanceType<typeof SqlExecutionLogDetail>>();
</script>
