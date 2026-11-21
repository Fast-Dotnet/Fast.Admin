<template>
	<FaDialog ref="dialogRef" :title="`${state.appName} · 下载二维码`" width="420" :show-fullscreen="false" :show-confirm-button="false">
		<div v-if="state.downloadUrl" class="application-download">
			<img class="application-download__qr" :src="state.qrCode" alt="应用下载二维码" />
			<p>使用手机扫描二维码下载</p>
			<el-input :model-value="state.downloadUrl" readonly />
			<el-button type="primary" @click="handleCopy">复制下载地址</el-button>
		</div>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { useFastAxios } from "@fast-china/axios";
import { copy } from "@fast-china/utils";
import { applicationApi } from "@/api/services/Center/application";
import type { FaDialogInstance } from "fast-element-plus";

defineOptions({
	name: "SystemApplicationDownload",
});

const fastAxios = useFastAxios();

const faDialogRef = useTemplateRef<FaDialogInstance>("dialogRef");

const state = reactive({
	appName: "",
	downloadUrl: "",
	qrCode: "",
});

const handleCopy = async () => {
	await copy(state.downloadUrl);
	ElMessage.success("下载地址已复制！");
};

const open = (appId: string, appName: string) => {
	faDialogRef.value.open(async () => {
		state.appName = appName;
		const apiRes = await applicationApi.queryAppDownload({ appId, baseUrl: fastAxios.baseUrl });
		state.downloadUrl = apiRes.downloadUrl;
		state.qrCode = apiRes.qrCode;
	});
};

defineExpose({
	element: faDialogRef,
	open,
});
</script>

<style lang="scss" scoped>
.application-download {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 16px;
	color: var(--el-text-color-secondary);
}

.application-download__qr {
	width: 240px;
	height: 240px;
	padding: 12px;
	background-color: #fff;
}
</style>
