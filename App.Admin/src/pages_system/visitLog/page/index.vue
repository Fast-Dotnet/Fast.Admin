<template>
	<FaPaging :request-api="visitLogApi.queryVisitLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput
				data-search-range="Past1M"
				v-model:search-param="searchParam.value"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			>
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaTenantSelect v-if="userInfoStore.isSuperAdmin" v-model="searchParam.value.tenantId" />
					<FaCellPicker title="访问类型" v-model="searchParam.value.visitType" name="VisitTypeEnum" radio clearable />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="detailFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="VisitTypeEnum" :value="item.visitType" />
					</view>
				</template>

				<FaField label="账号" :value="[item.nickName, item.mobile].filter(Boolean).join(' · ')" />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="地区" :value="[item.province, item.city].filter(Boolean).join(' - ')" />
				<FaField label="Ip" :value="item.ip" />
				<FaField label="设备" :value="[item.device, item.os].filter(Boolean).join(' · ')" />
				<FaField label="浏览器" :value="item.browser" />
				<FaField v-if="userInfoStore.isSuperAdmin" label="租户" :value="item.tenantName" />
			</FaCard>
		</template>
	</FaPaging>

	<VisitLogDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { visitLogApi } from "@/api/services/Center/visitLog";
import { useUserInfo } from "@/stores";
import VisitLogDetail from "./components/detail.vue";

definePage({
	name: "SystemVisitLog",
	authCode: "VisitLog:Paged",
	style: {
		navigationBarTitleText: "访问日志",
	},
});

const userInfoStore = useUserInfo();

const detailFormRef = ref<InstanceType<typeof VisitLogDetail>>();
</script>
