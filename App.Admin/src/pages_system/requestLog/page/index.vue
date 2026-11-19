<template>
	<FaPaging page-key="RequestLog" :request-api="requestLogApi.queryRequestLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaTenantSelect v-if="userInfoStore.isSystemTenant" v-model="searchParam.value.tenantId" />
					<FaCellPicker title="成功" v-model="searchParam.value.isSuccess" name="BooleanEnum" radio clearable />
					<FaCellPicker title="操作方式" v-model="searchParam.value.operationAction" name="HttpRequestActionEnum" radio clearable />
					<FaCellPicker title="请求方式" v-model="searchParam.value.requestMethod" name="HttpRequestMethodEnum" radio clearable />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.operationName" @click="detailFormRef.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="HttpRequestMethodEnum" :value="item.requestMethod" />
						<wd-tag v-if="item.isSuccess" type="success">成功</wd-tag>
						<wd-tag v-else type="danger">失败</wd-tag>
					</view>
				</template>
				<FaField label="账号" :value="[item.nickName, item.mobile].filter(Boolean).join(' · ')" />
				<FaField label="请求地址" :value="item.location" copyable />
				<FaField label="耗时" :value="`${(Number(item.elapsedTime ?? 0) / 1000).toFixed(4)}秒`" color="var(--wot-text-auxiliary)" />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="地区" :value="[item.province, item.city].filter(Boolean).join(' - ')" />
				<FaField label="Ip" :value="item.ip" copyable />
				<FaField label="设备" :value="[item.device, item.os].filter(Boolean).join(' · ')" />
				<FaField label="浏览器" :value="item.browser" />
				<FaField label="时间" :value="item.createdTime" />
				<FaField v-if="userInfoStore.isSystemTenant" label="租户" :value="item.tenantName" />
			</FaCard>
		</template>
	</FaPaging>

	<RequestLogDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { requestLogApi } from "@/api/services/Center/requestLog";
import { useUserInfo } from "@/stores";
import RequestLogDetail from "./components/detail.vue";

definePage({
	name: "SystemRequestLog",
	authCode: "RequestLog:Paged",
	style: {
		navigationBarTitleText: "请求日志",
	},
});

const userInfoStore = useUserInfo();

const detailFormRef = ref<InstanceType<typeof RequestLogDetail>>();
</script>
