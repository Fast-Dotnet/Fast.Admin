<template>
	<FaPaging page-key="Api" :request-api="apiApi.queryApiPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()" />
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.apiId" :title="item.apiName" @click="detailFormRef.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="HttpRequestActionEnum" :value="item.action" />
						<FaTag name="HttpRequestMethodEnum" :value="item.method" />
					</view>
				</template>

				<view style="color: var(--wot-primary-6); overflow-wrap: anywhere">
					{{ item.apiUrl }}
				</view>
				<FaField label="模块名称" :value="item.moduleName" />
				<FaField label="服务名称" :value="item.serviceName" />
				<FaField label="分组名称" :value="item.groupName" />
				<FaField label="分组" :value="item.groupTitle" />
				<view v-if="item.hasAuth || item.hasPermission" style="display: flex; gap: 16rpx; margin-top: 8rpx">
					<wd-tag v-if="item.hasAuth" type="primary">需登录</wd-tag>
					<wd-tag v-if="item.hasPermission" type="warning">权限校验</wd-tag>
				</view>
			</FaCard>
		</template>
	</FaPaging>

	<ApiDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { apiApi } from "@/api/services/Center/api";
import ApiDetail from "./components/detail.vue";

definePage({
	name: "DevApi",
	authCode: "Api:Paged",
	style: {
		navigationBarTitleText: "接口管理",
	},
});

const detailFormRef = ref<InstanceType<typeof ApiDetail>>();
</script>
