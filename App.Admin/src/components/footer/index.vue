<template>
	<view :class="['fa-footer', { 'fa-footer--filing': hasFiling }]">
		<text>FastDotnet 提供计算服务</text>
		<text>© {{ new Date().getFullYear() }} {{ domain }} v{{ appStore.appVersion }}</text>
		<!-- #ifdef H5 -->
		<view v-if="hasFiling" class="fa-footer__filing">
			<a
				v-if="appStore.publicSecurityCode"
				class="fa-footer__link"
				:href="`https://beian.mps.gov.cn/#/query/webSearch?code=${appStore.publicSecurityCode.replace(/\D+/g, '')}`"
				target="_blank"
				rel="noopener noreferrer"
			>
				<image class="fa-footer__filing-icon" :src="gonganImage" mode="aspectFit" />
				<text>{{ appStore.publicSecurityCode }}</text>
			</a>
			<a v-if="appStore.icpSecurityCode" class="fa-footer__link" href="https://beian.miit.gov.cn/" target="_blank" rel="noopener noreferrer">
				{{ appStore.icpSecurityCode }}
			</a>
		</view>
		<!-- #endif -->
	</view>
</template>

<script setup lang="ts">
import { computed } from "vue";
// #ifdef H5
import gonganImage from "@/static/images/gongan.png";
// #endif
import { useApp } from "@/stores";

defineOptions({
	// eslint-disable-next-line vue/no-reserved-component-names -- 保持现有组件名称与项目引用兼容
	name: "Footer",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const appStore = useApp();

const hasFiling = computed(() => process.env.UNI_PLATFORM === "h5" && !!(appStore.icpText || appStore.publicSecurityCode));

let domain = "";
const host = import.meta.env.VITE_API_REQUEST_URL.replace(/^https?:\/\//, "").split(/[/:]/)[0];
// IPv4 和 localhost 原样展示；普通域名仅保留最后两段。
if (/^\d{1,3}(?:\.\d{1,3}){3}$/.test(host) || host === "localhost") {
	domain = host;
} else {
	const hostParts = host.split(".");
	domain =
		hostParts.length >= 2
			? hostParts.slice(-2).join(".") // example.com
			: host; // 单段主机名
}
</script>
