<template>
	<view class="page">
		<FaSearchInput
			class="mb20"
			search
			:filter="false"
			v-model="state.searchValue"
			@confirm="handleSearch()"
			@clear="handleSearch"
			@search="handleSearch"
		/>
		<wd-swiper custom-class="mb20" :list="state.swiperList" autoplay v-model:current="state.swiperCurrent" :indicator="{ type: 'dots-bar' }" />
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh, onShareAppMessage, onShareTimeline, onShow } from "@dcloudio/uni-app";
import { reactive, watch } from "vue";
import { Local } from "@fast-china/utils";
import { CommonRoute } from "@/common";
import bannerAdmin from "@/static/images/banner-fast-admin.jpg";
import bannerAnywhere from "@/static/images/banner-fast-anywhere.jpg";
import bannerInsights from "@/static/images/banner-fast-insights.jpg";
import bannerSecure from "@/static/images/banner-fast-secure.jpg";
import bannerTogether from "@/static/images/banner-fast-together.jpg";
import defaultLogo from "@/static/logo.png";
import { useApp, useUserInfo } from "@/stores";

definePage({
	name: "Home",
	layout: "layout",
	isTabBar: true,
	style: {
		navigationBarTitleText: "首页",
		enablePullDownRefresh: true,
	},
});

const appStore = useApp();
const userInfoStore = useUserInfo();

const state = reactive({
	swiperList: [bannerAdmin, bannerInsights, bannerTogether, bannerAnywhere, bannerSecure],
	swiperCurrent: 0,
	/** 搜素值 */
	searchValue: "",
});

/** 搜素 */
const handleSearch = () => {
	if (!state.searchValue) return;
};

/** 加载页面 */
const loadPage = () => {
	console.log("加载页面");
};

/** 页面刷新 */
const loadRefresh = () => {
	console.log("页面刷新");
};

watch(
	() => appStore.appName,
	(newVal) => {
		uni.setNavigationBarTitle({
			title: newVal ?? "首页",
		});
	},
	{
		immediate: true,
	}
);

onLoad(() => {
	loadPage();
});

onShow(() => {
	loadRefresh();
});

onPullDownRefresh(async () => {
	loadPage();
	loadRefresh();
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
