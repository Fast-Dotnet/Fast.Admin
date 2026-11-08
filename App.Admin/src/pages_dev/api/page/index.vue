<template>
	<FaPaging :request-api="apiApi.queryApiPaged">
		<template #top="{ searchParam, setParam, refresh }">
			<FaSearchInput
				:filter="false"
				:search-param="searchParam"
				placeholder="搜索接口地址、名称、模块"
				@update:search-param="setParam"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			/>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.apiId" :title="item.apiName" @click="editFormRef.detail(item)">
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
				<FaField label="分组标题" :value="item.groupTitle" />
				<FaField label="版本" :value="item.version" />
				<view style="display: flex; gap: 16rpx; margin-top: 8rpx">
					<wd-tag v-if="item.hasAuth" type="primary">需登录</wd-tag>
					<wd-tag v-if="item.hasPermission" type="warning">权限校验</wd-tag>
				</view>
			</FaCard>
		</template>
	</FaPaging>

	<ApiDetail ref="editFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { apiApi } from "@/api/services/Center/api";
import ApiDetail from "./detail/index.vue";

definePage({
	name: "DevApi",
	layout: "layout",
	authCode: "Api:Paged",
	style: {
		navigationBarTitleText: "接口管理",
	},
});

const editFormRef = ref<InstanceType<typeof ApiDetail>>();
</script>
