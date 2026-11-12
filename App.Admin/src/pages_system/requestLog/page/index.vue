<template>
	<FaPaging :request-api="requestLogApi.queryRequestLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaTenantSelect v-if="userInfoStore.isSuperAdmin" v-model="searchParam.value.tenantId" />
					<FaCellPicker title="成功" v-model="searchParam.value.isSuccess" name="BooleanEnum" radio />
					<FaCellPicker title="操作方式" v-model="searchParam.value.operationAction" name="HttpRequestActionEnum" radio />
					<FaCellPicker title="请求方式" v-model="searchParam.value.requestMethod" name="HttpRequestMethodEnum" radio />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="editFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="HttpRequestActionEnum" :value="item.operationAction" />
						<FaTag name="HttpRequestMethodEnum" :value="item.requestMethod" />
						<FaTag name="BooleanEnum" :value="item.isSuccess" />
					</view>
				</template>
				<FaField label="昵称" :value="`${item.nickName} · ${item.mobile}`" />
				<FaField label="操作名称" :value="item.operationName" />
				<FaField label="请求地址" :value="item.location" />
				<FaField label="耗时" :value="`${(Number(item.elapsedTime ?? 0) / 1000).toFixed(4)}秒`" color="var(--wot-text-auxiliary)" />
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

	<RequestLogDetail ref="editFormRef" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import { requestLogApi } from "@/api/services/Center/requestLog";
import { useUserInfo } from "@/stores";
import RequestLogDetail from "./detail/index.vue";

definePage({
	name: "SystemRequestLog",
	authCode: "RequestLog:Paged",
	style: {
		navigationBarTitleText: "请求日志",
	},
});

const userInfoStore = useUserInfo();
const editFormRef = ref<InstanceType<typeof RequestLogDetail>>();
</script>
