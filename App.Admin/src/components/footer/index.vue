<template>
	<view class="fa-footer">
		<text>FastDotnet 提供计算服务</text>
		<text>© {{ new Date().getFullYear() }} {{ domain }} v{{ appStore.appVersion }}</text>
	</view>
</template>

<script setup lang="ts">
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

let domain = "";
const host = import.meta.env.VITE_API_REQUEST_URL.replace(/^https?:\/\//, "").split(/[/:]/)[0];
// ip 或者 localhost
if (/^\d{1,3}(?:\.\d{1,3}){3}$/.test(host) || host === "localhost") {
	domain = host;
} else {
	const hostParts = host.split(".");
	domain =
		hostParts.length >= 2
			? hostParts.slice(-2).join(".") // example.com
			: host; // localhost 或 IP
}
</script>

<style lang="scss" scoped>
.fa-footer {
	height: var(--fa-footer-height, 40px);
	font-size: var(--wot-typography-label-size-large);
	color: var(--wot-text-auxiliary);
	letter-spacing: 0.5px;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	transition: height var(--fa-transition-duration);
	overflow: hidden;
}
</style>
