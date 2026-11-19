<template>
	<FaPopup ref="faPopupRef" title="文件存储详情" height="70%" position="bottom" closable>
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

		<view class="page-detail__popup">
			<FaField label="标识" :value="state.formData.fileObjectName" copyable />
			<FaField label="后缀" :value="state.formData.fileSuffix" selectable />
			<FaField label="类型" :value="state.formData.fileMimeType" selectable />
			<FaField label="大小" :value="formatFileSize(state.formData.fileSizeKb)" selectable />
			<FaField label="存储路径" :value="state.formData.filePath" copyable />
			<FaField label="访问地址" :value="state.formData.fileLocation" copyable />
			<FaField label="哈希" :value="state.formData.fileHash" copyable />
			<FaField label="名称" :value="state.formData.fileOriginName" selectable />
			<FaField label="省份" :value="state.formData.uploadProvince" selectable />
			<FaField label="城市" :value="state.formData.uploadCity" selectable />
			<FaField label="Ip" :value="state.formData.uploadIp" selectable />
			<FaField label="设备" :value="state.formData.uploadDevice" selectable />
			<FaField label="操作系统" :value="state.formData.uploadOS" selectable />
			<FaField label="浏览器" :value="state.formData.uploadBrowser" selectable />
			<FaField label="创建人" :value="state.formData.createdUserName" selectable />
			<FaField label="创建时间" :value="state.formData.createdTime" selectable />
			<FaField v-if="userInfoStore.isSystemTenant" label="租户" :value="state.formData.tenantName" selectable />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import Decimal from "decimal.js";
import { useUserInfo } from "@/stores";
import type { QueryFilePagedOutput } from "@/api/services/File/models/QueryFilePagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemFileDetail",
});

const userInfoStore = useUserInfo();

const faPopupRef = ref<FaPopupInstance>();

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

const detail = throttle(
	async (data: QueryFilePagedOutput) =>
		await faPopupRef.value.open(() => {
			state.formData = data;
		})
);

defineExpose({
	detail,
});
</script>
