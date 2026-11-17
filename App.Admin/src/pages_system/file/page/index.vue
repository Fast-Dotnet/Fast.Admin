<template>
	<FaPaging :request-api="fileApi.queryFilePaged">
		<template v-if="userInfoStore.isSystemTenant" #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaTenantSelect v-model="searchParam.value.tenantId" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.fileId" :title="item.fileObjectName" @click="editFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<wd-tag type="primary" variant="plain">{{ item.fileSuffix }}</wd-tag>
					</view>
				</template>

				<FaField label="原文件名" :value="item.fileOriginName" />
				<FaField label="大小" :value="formatFileSize(item.fileSizeKb)" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField v-if="userInfoStore.isSystemTenant" label="租户" :value="item.tenantName" />

				<template #footer>
					<wd-button
						v-if="state.imageMimeType.includes(item.fileMimeType)"
						type="info"
						size="small"
						variant="plain"
						@click="handlePreview(item.fileLocation)"
					>
						预览
					</wd-button>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<FileDetail ref="editFormRef" />
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { Decimal } from "decimal.js";
import { fileApi } from "@/api/services/File";
import { useUserInfo } from "@/stores";
import FileDetail from "./components/detail.vue";

definePage({
	name: "SystemFile",
	authCode: "File:Paged",
	style: {
		navigationBarTitleText: "文件存储",
	},
});

const userInfoStore = useUserInfo();

const editFormRef = ref<InstanceType<typeof FileDetail>>();

const state = reactive({
	imageMimeType: ["image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp"],
});

const handlePreview = (url: string) => {
	uni.previewImage({ urls: [url], current: url });
};

const formatFileSize = (value?: string) => {
	let size = Decimal(value ?? 0);
	const units = ["KB", "MB", "GB", "TB"];
	let unit = 0;
	while (size.gte(1024) && unit < units.length - 1) {
		size = size.div(1024);
		unit++;
	}
	return `${size.toDecimalPlaces(2).toFixed()} ${units[unit]}`;
};
</script>
