<template>
	<view class="page">工作台</view>
</template>

<script setup lang="ts">
import { onPullDownRefresh, onShareAppMessage, onShareTimeline } from "@dcloudio/uni-app";
import { Local } from "@fast-china/utils";
import { CommonRoute } from "@/common";
import defaultLogo from "@/static/logo.png";
import { useApp, useUserInfo } from "@/stores";

definePage({
	name: "Workbench",
	layout: "layout",
	isTabBar: true,
	style: {
		navigationBarTitleText: "工作台",
		enablePullDownRefresh: true,
	},
});

const appStore = useApp();
const userInfoStore = useUserInfo();

onPullDownRefresh(async () => {
	// 删除 HTTP 缓存数据
	Local.removeByPrefix("HTTP_CACHE_");
	// 刷新用户信息
	await userInfoStore.refreshUserInfo();
	// 刷新字典
	await appStore.setDictionary();
	uni.stopPullDownRefresh();
});

// #ifdef MP-WEIXIN
onShareAppMessage(() => ({
	title: appStore.appName,
	path: CommonRoute.Launcher,
	imageUrl: defaultLogo,
}));

onShareTimeline(() => ({
	title: appStore.appName,
	path: CommonRoute.Launcher,
	imageUrl: defaultLogo,
}));
// #endif
</script>

<style scoped lang="scss">
@use "./index.scss";
</style>
