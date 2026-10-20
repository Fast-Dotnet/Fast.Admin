<template>
	<div>
		<FaTable row-key="fileId" :request-api="fileApi.queryFilePaged" :columns="searchColumns">
			<FaTableColumn prop="fileObjectName" label="唯一标识" fixed="left" width="200" small-width="180" copy sortable />
			<FaTableColumn prop="fileSuffix" label="后缀" width="100" small-width="80" sortable tag />
			<FaTableColumn prop="fileMimeType" label="Mime类型" width="120" small-width="100" sortable tag />
			<FaTableColumn prop="fileSizeKb" label="大小" width="100" small-width="80" sortable>
				<template #default="{ row }: { row: QueryFilePagedOutput }">
					{{ formatFileSize(row.fileSizeKb) }}
				</template>
			</FaTableColumn>
			<FaTableColumn prop="filePath" label="存储路径" width="200" small-width="180" sortable />
			<FaTableColumn prop="fileLocation" label="访问地址" width="300" small-width="280" copy sortable />
			<FaTableColumn prop="fileHash" label="哈希" width="300" small-width="280" sortable />
			<FaTableColumn prop="fileOriginName" label="名称" width="200" small-width="180" sortable />
			<FaTableColumn prop="uploadDevice" label="设备" width="150" small-width="130" sortable />
			<FaTableColumn prop="uploadOS" label="操作系统" width="150" small-width="130" sortable />
			<FaTableColumn prop="uploadBrowser" label="浏览器" width="150" small-width="130" sortable />
			<FaTableColumn prop="uploadProvince" label="省份" width="120" small-width="100" sortable />
			<FaTableColumn prop="uploadCity" label="城市" width="120" small-width="100" sortable />
			<FaTableColumn prop="uploadIp" label="Ip" width="150" small-width="130" sortable />
			<FaTableColumn prop="tenantName" label="租户" width="200" small-width="180" sortable />
			<FaTableColumn prop="createdTime" label="创建时间" width="240" small-width="220" sortable type="timeInfo" />

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryFilePagedOutput }">
				<el-button v-if="state.imageMimeType.includes(row.fileMimeType)" size="small" plain @click="state.previewSrc = row.fileLocation">
					预览
				</el-button>
			</template>
		</FaTable>
		<el-image-viewer
			v-if="state.previewSrc"
			:url-list="[state.previewSrc]"
			hide-on-click-modal
			teleported
			show-progress
			@close="state.previewSrc = ''"
		/>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive } from "vue";
import { fileApi } from "@/api/services/File";
import { useUserInfo } from "@/stores";
import type { FaTableColumnCtx } from "fast-element-plus";
import type { QueryFilePagedOutput } from "@/api/services/File/models/QueryFilePagedOutput";

defineOptions({
	name: "SystemFile",
});

const userInfoStore = useUserInfo();

const state = reactive({
	imageMimeType: ["image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp"],
	previewSrc: "",
});

const searchColumns = computed<FaTableColumnCtx[]>(() =>
	userInfoStore.isSuperAdmin
		? [
				{
					pureSearch: true,
					prop: "tenantId",
					label: "租户",
					search: { el: "TenantSelectPage" },
				},
			]
		: []
);

const formatFileSize = (value?: string) => {
	const kb = Number(value ?? 0);
	if (kb < 1024) return `${kb} KB`;
	const mb = kb / 1024;
	if (mb < 1024) return `${mb.toFixed(2)} MB`;
	return `${(mb / 1024).toFixed(2)} GB`;
};
</script>
