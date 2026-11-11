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
					<FaCellPicker title="访问类型" v-model="searchParam.value.visitType" name="VisitTypeEnum" radio />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="editFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="VisitTypeEnum" :value="item.visitType" />
					</view>
				</template>

				<FaField label="昵称" :value="`${item.nickName} · ${item.mobile}`" />
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

	<VisitLogDetail ref="editFormRef" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import { visitLogApi } from "@/api/services/Center/visitLog";
import { useUserInfo } from "@/stores";
import VisitLogDetail from "./detail/index.vue";

definePage({
	name: "SystemVisitLog",
	layout: "layout",
	authCode: "VisitLog:Paged",
	style: {
		navigationBarTitleText: "访问日志",
	},
});

const userInfoStore = useUserInfo();

const editFormRef = ref<InstanceType<typeof VisitLogDetail>>();
</script>
