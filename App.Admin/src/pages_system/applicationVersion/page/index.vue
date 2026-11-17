<template>
	<FaPaging page-key="ApplicationVersion" :request-api="applicationVersionApi.queryApplicationVersionPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="包类型" v-model="searchParam.value.packageType" name="ApplicationPackageTypeEnum" radio clearable />
					<FaCellPicker title="状态" v-model="searchParam.value.status" name="CommonStatusEnum" radio clearable />
				</template>

				<template #footer>
					<FaAuth code="AppVersion:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.recordId"
				:title="item.appKey"
				code="AppVersion:Detail"
				@click="detailFormRef.detail(item.recordId)"
			>
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="ApplicationPackageTypeEnum" :value="item.packageType" />
					</view>
				</template>

				<FaField label="应用名称" :value="item.appName" />
				<FaField label="应用类型">
					<FaTagFlag name="AppEnvironmentEnum" :value="item.appType" />
				</FaField>
				<FaField label="版本名称" :value="item.versionName" />
				<FaField label="版本号" :value="item.versionCode" />
				<FaField label="状态">
					<FaTag name="CommonStatusEnum" :value="item.status" />
				</FaField>

				<template #footer>
					<FaAuth code="AppVersion:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.recordId)">编辑</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<ApplicationVersionDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { applicationVersionApi } from "@/api/services/Center/applicationVersion";
import ApplicationVersionDetail from "./components/detail.vue";

definePage({
	name: "SystemApplicationVersion",
	authCode: "AppVersion:Paged",
	style: {
		navigationBarTitleText: "应用版本",
	},
});

const router = useRouter();

const detailFormRef = ref<InstanceType<typeof ApplicationVersionDetail>>();

const handleEdit = throttle(
	async (mode: IPageStateType, recordId?: string) =>
		await router.push({
			path: "/pages_system/applicationVersion/edit/index",
			query: {
				mode,
				recordId,
			},
		})
);
</script>
