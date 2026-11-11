<template>
	<FaPaging :request-api="fileApi.queryFilePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaTenantSelect v-if="userInfoStore.isSuperAdmin" v-model="searchParam.value.tenantId" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.fileId" :title="item.fileObjectName" @click="editFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<wd-tag>{{ item.fileMimeType }}</wd-tag>
					</view>
				</template>

				<FaField label="后缀">
					<wd-tag>{{ item.fileSuffix }}</wd-tag>
				</FaField>
				<FaField label="大小" :value="formatFileSize(item.fileSizeKb)" />
				<FaField label="名称" :value="item.fileOriginName" />
				<FaField label="地区" :value="`${item.uploadProvince} - ${item.uploadCity}`" />
				<FaField label="Ip" :value="item.uploadIp" />
				<FaField label="设备" :value="item.uploadDevice" />
				<FaField label="操作系统" :value="item.uploadOS" />
				<FaField label="浏览器" :value="item.uploadBrowser" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="租户" :value="item.tenantName" />

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
import FileDetail from "./detail/index.vue";

definePage({
	name: "SystemFile",
	layout: "layout",
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
