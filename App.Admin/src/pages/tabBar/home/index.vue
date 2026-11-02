<template>
	<view class="page">
		<view class="greeting">
			<view class="greeting__content">
				<view class="greeting__title">{{ getLocalTimeGreeting() }}{{ userInfoStore.nickName }}</view>
				<view class="greeting__description">让日常工作，更简单一点</view>
			</view>
			<FaImage width="96rpx" height="96rpx" round :src="userInfoStore.avatar" />
		</view>

		<wd-swiper :list="state.swiperList" autoplay v-model:current="state.swiperCurrent" :indicator="{ type: 'dots-bar' }" />

		<view class="fa-card">
			<view class="workspace__header">
				<text class="workspace__title">当前工作空间</text>
				<wd-button type="primary" variant="text" size="small" @click="router.pushTab(CommonRoute.My)">管理</wd-button>
			</view>
			<view class="workspace__body">
				<view class="workspace__icon">
					<FaImage v-if="userInfoStore.logoUrl" width="80rpx" height="80rpx" :src="userInfoStore.logoUrl" />
					<FaIcon v-else name="organization" size="44rpx" />
				</view>
				<view class="workspace__info">
					<view class="workspace__name">{{ userInfoStore.tenantName }}</view>
					<view class="workspace__description">
						{{ userInfoStore.employeeName }} · {{ userInfoStore.departmentName || "当前工作空间" }}
					</view>
				</view>
			</view>
			<wd-button custom-class="workspace__action" type="primary" @click="router.pushTab(CommonRoute.Workbench)">
				<view class="workspace__action-content">
					<FaIcon name="menu" size="32rpx" />
					<text>进入工作台</text>
					<wd-icon name="arrow-right" />
				</view>
			</wd-button>
		</view>

		<view class="help">
			<view class="help__item" @click="router.push('/pages/setting/account/index')">
				<FaIcon name="accountSafe" size="38rpx" color="var(--wot-primary-6)" />
				<text>账号安全</text>
				<wd-icon name="arrow-right" />
			</view>
			<view class="help__item" @click="router.push(CommonRoute.ComplaintSubmit)">
				<FaIcon name="message" size="38rpx" color="var(--wot-success-main)" />
				<text>意见反馈</text>
				<wd-icon name="arrow-right" />
			</view>
		</view>

		<view class="footer">FASTDOTNET · 简单工作，高效协同</view>
	</view>
</template>

<script setup lang="ts">
import { onPullDownRefresh, onShareAppMessage, onShareTimeline } from "@dcloudio/uni-app";
import { reactive, watch } from "vue";
import { Local, getLocalTimeGreeting } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
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

const router = useRouter();

const state = reactive({
	swiperList: [bannerAdmin, bannerInsights, bannerTogether, bannerAnywhere, bannerSecure],
	swiperCurrent: 0,
	/** 搜素值 */
	searchValue: "",
});

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

onPullDownRefresh(async () => {
	try {
		// 删除 HTTP 缓存数据
		Local.removeByPrefix("HTTP_CACHE_");
		// 刷新用户信息
		await userInfoStore.refreshUserInfo();
		// 刷新字典
		await appStore.setDictionary();
	} finally {
		uni.stopPullDownRefresh();
	}
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

<style lang="scss" scoped>
@use "./index.scss";
</style>
