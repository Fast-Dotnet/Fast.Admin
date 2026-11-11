<template>
	<FaPopup ref="popupRef" title="文件存储详情" height="70%" position="bottom" closable>
		<template #titlePrefix>
			<wd-button
				v-if="state.imageMimeType.includes(state.formData.fileMimeType)"
				type="info"
				size="small"
				variant="plain"
				@click="handlePreview"
			>
				预览
			</wd-button>
		</template>
		<scroll-view scroll-y>
			<view style="display: flex; flex-direction: column; gap: 10px">
				<FaField label="标识" :value="state.formData.fileObjectName" selectable />
				<FaField label="后缀" :value="state.formData.fileSuffix" />
				<FaField label="类型" :value="state.formData.fileMimeType" />
				<FaField label="大小" :value="formatFileSize(state.formData.fileSizeKb)" />
				<FaField label="存储路径" :value="state.formData.filePath" selectable />
				<FaField label="访问地址" :value="state.formData.fileLocation" selectable />
				<FaField label="哈希" :value="state.formData.fileHash" selectable />
				<FaField label="名称" :value="state.formData.fileOriginName" />
				<FaField label="地区" :value="`${state.formData.uploadProvince} - ${state.formData.uploadCity}`" />
				<FaField label="Ip" :value="state.formData.uploadIp" />
				<FaField label="设备" :value="state.formData.uploadDevice" />
				<FaField label="操作系统" :value="state.formData.uploadOS" />
				<FaField label="浏览器" :value="state.formData.uploadBrowser" />
				<FaField label="创建人" :value="state.formData.createdUserName" />
				<FaField label="创建时间" :value="state.formData.createdTime" />
				<FaField label="租户" :value="state.formData.tenantName" />
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
import Decimal from "decimal.js";
import type { QueryFilePagedOutput } from "@/api/services/File/models/QueryFilePagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemFileDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<QueryFilePagedOutput>({}),
	imageMimeType: ["image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp"],
});

const handlePreview = () => {
	uni.previewImage({ urls: [state.formData.fileLocation], current: state.formData.fileLocation });
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

const detail = (data: QueryFilePagedOutput) => {
	popupRef.value?.open(() => {
		state.formData = data;
	});
};

defineExpose({
	detail,
});
</script>
