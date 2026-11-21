<template>
	<FaPopup ref="faPopupRef" :title="`${state.appName} · 下载二维码`" position="center" width="80%" closable>
		<view v-if="state.downloadUrl" class="application-download">
			<image class="application-download__qr" :src="state.qrCode" mode="aspectFit" show-menu-by-longpress />
			<text>扫描二维码，或复制地址在浏览器中打开</text>
			<text class="application-download__address" selectable>{{ state.downloadUrl }}</text>
			<wd-button block @click="handleCopy">复制下载地址</wd-button>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useFastAxios } from "@fast-china/axios";
import { throttle } from "@fast-china/utils";
import { applicationApi } from "@/api/services/Center/application";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemApplicationDownload",
});

const fastAxios = useFastAxios();

const faPopupRef = ref<FaPopupInstance>();

const state = reactive({
	appName: "",
	downloadUrl: "",
	qrCode: "",
});

const handleCopy = async () => {
	await uni.setClipboardData({ data: state.downloadUrl });
};

const open = throttle(
	async (appId: string, appName: string) =>
		await faPopupRef.value.open(async () => {
			state.appName = appName;
			const apiRes = await applicationApi.queryAppDownload({ appId, baseUrl: fastAxios.baseUrl });
			state.downloadUrl = apiRes.downloadUrl;
			state.qrCode = apiRes.qrCode;
		})
);

defineExpose({
	open,
});
</script>

<style scoped lang="scss">
.application-download {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 16px;
	color: var(--wot-text-secondary);
	font-size: 12px;
	text-align: center;
}

.application-download__qr {
	width: 440rpx;
	height: 440rpx;
	padding: 12px;
	background-color: #fff;
}

.application-download__address {
	width: 100%;
	overflow-wrap: anywhere;
	text-align: center;
}
</style>
