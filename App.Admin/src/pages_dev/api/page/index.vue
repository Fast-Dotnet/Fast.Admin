<template>
	<view class="page">
		<z-paging ref="paging" v-model="pagedList" @query="pagingQuery">
			<template #top>
				<FaSearchInput
					:filter="false"
					v-model:search-param="searchParam"
					placeholder="搜索接口地址、名称、模块"
					@confirm="pagingRefresh()"
					@clear="pagingRefresh()"
					@search="pagingRefresh()"
				/>
			</template>

			<wd-card v-for="item in pagedList" :key="item.apiId" custom-class="wd-card__page">
				<template #title>
					<text class="wd-card__title-text">{{ item.apiName }}</text>
					<view class="wd-card__title-view">
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

				<template #footer>
					<wd-button size="small" type="info" variant="plain" @click="editFormRef.detail(item)">详情</wd-button>
				</template>
			</wd-card>
		</z-paging>

		<DevApiDetail ref="editFormRef" />
	</view>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { apiApi } from "@/api/services/Center/api";
import { usePaging } from "@/hooks";
import DevApiDetail from "./detail/index.vue";

definePage({
	name: "DevApi",
	layout: "layout",
	authCode: "Api:Paged",
	style: {
		navigationBarTitleText: "接口管理",
	},
});

const { paging, pagedList, searchParam, pagingQuery, pagingRefresh } = usePaging((input) => apiApi.queryApiPaged(input));

const editFormRef = ref<InstanceType<typeof DevApiDetail>>();
</script>
