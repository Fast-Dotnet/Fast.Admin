<template>
	<FaPaging :request-api="sqlDiffLogApi.querySqlDiffLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaTenantSelect v-model="searchParam.value.tenantId" />
					<FaCellPicker title="差异类型" v-model="searchParam.value.diffType" name="DiffLogTypeEnum" radio />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="editFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="DiffLogTypeEnum" :value="item.diffType" />
					</view>
				</template>

				<FaField label="昵称" :value="`${item.nickName} · ${item.mobile}`" />
				<FaField label="表名称" :value="item.tableName" />
				<FaField label="表描述" :value="item.tableDescription" />
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

	<SqlDiffLogDetail ref="editFormRef" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import { sqlDiffLogApi } from "@/api/services/Center/sqlDiffLog";
import SqlDiffLogDetail from "./detail/index.vue";

definePage({
	name: "DevSqlDiffLog",
	layout: "layout",
	authCode: "SqlDiffLog:Paged",
	style: {
		navigationBarTitleText: "Sql差异日志",
	},
});

const editFormRef = ref<InstanceType<typeof SqlDiffLogDetail>>();
</script>
