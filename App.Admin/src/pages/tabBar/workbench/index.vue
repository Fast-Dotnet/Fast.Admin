<template>
	<view class="page">
		<view class="workbench-header">
			<FaSearchInput v-model="state.searchValue" :filter="false" placeholder="搜索应用名称" />
			<view class="workspace" @click="switchTenantPopupRef?.open()">
				<view class="workspace__name">
					<view class="workspace__logo">
						<FaImage
							width="48rpx"
							height="48rpx"
							:src="userInfoStore.logoUrl || defaultLogo"
							original
							:hide-image="false"
							:enable-preview="false"
						/>
					</view>
					<text class="workspace__title">{{ userInfoStore.tenantName }}</text>
				</view>
				<text class="workspace__department">{{ userInfoStore.departmentName || "当前工作空间" }}</text>
				<wd-icon name="arrow-down" size="24rpx" color="var(--wot-text-secondary)" />
			</view>
		</view>

		<view class="workbench-content">
			<view v-if="!state.searchValue.trim()" class="page-card menu-section">
				<view class="section-header">
					<view class="section-title">
						<wd-icon name="star" size="32rpx" color="var(--wot-primary-6)" />
						常用应用
					</view>
					<wd-button type="primary" variant="text" size="small" @click="commonMenuPopupRef?.open(commonMenuList)">编辑</wd-button>
				</view>

				<MenuGrid v-if="commonMenuList.length" :menus="commonMenuList" @select="handleMenuClick" />

				<view v-else class="menu-empty" @click="commonMenuPopupRef?.open(commonMenuList)">点击编辑，添加你的常用应用</view>
			</view>

			<view v-for="group in filteredMenuList" :key="group.menuCode" class="page-card menu-section">
				<view class="section-header">
					<view class="section-title">
						<view class="section-marker" :style="{ backgroundColor: getMarkerColor(group) }" />
						<text>{{ group.menuTitle || group.menuName || group.menuCode }}</text>
					</view>
					<text class="section-count">{{ group.children.length }} 个应用</text>
				</view>
				<MenuGrid :menus="group.children" @select="handleMenuClick" />
			</view>
			<wd-empty v-if="!filteredMenuList.length" :tip="state.searchValue.trim() ? '没有找到相关应用' : '暂无可访问的应用'" />
		</view>

		<FaSwitchTenantPopup ref="switchTenantPopupRef" @change="loadPage" />
		<CommonMenuPopup ref="commonMenuPopupRef" :menus="menuList" @ok="loadPage" />
	</view>
</template>

<script setup lang="ts">
import { onPullDownRefresh, onShareAppMessage, onShareTimeline, onShow } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { Local, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { employeeApi } from "@/api/services/Admin/employee/index.ts";
import { CommonRoute } from "@/common";
import defaultLogo from "@/static/logo.png";
import { useApp, useUserInfo } from "@/stores";
import CommonMenuPopup from "./components/CommonMenuPopup.vue";
import MenuGrid from "./components/MenuGrid.vue";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";
import type { FaSwitchTenantPopupInstance } from "@/components";

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

const router = useRouter();

const switchTenantPopupRef = ref<FaSwitchTenantPopupInstance>();
const commonMenuPopupRef = ref<InstanceType<typeof CommonMenuPopup>>();

const state = reactive({
	searchValue: "",
	commonMenuCodes: withDefineType<string[]>([]),
});

const menuList = computed(() =>
	userInfoStore.menuList
		.filter((f) => f.visible)
		.map((item) => ({
			...item,
			children: (item.children ?? []).filter((menu) => menu.visible),
		}))
);
const commonMenuList = computed(() =>
	menuList.value.flatMap((item) => item.children).filter((menu) => state.commonMenuCodes.includes(menu.menuCode))
);
const filteredMenuList = computed(() => {
	const keyword = state.searchValue.trim().toLocaleLowerCase();
	return menuList.value
		.map((item) => ({
			...item,
			children: item.children.filter((menu) => menu.menuName.toLocaleLowerCase().includes(keyword)),
		}))
		.filter((item) => item.children.length);
});

const markerColors = [
	"#3b82f6",
	"#22c55e",
	"#f59e0b",
	"#ef4444",
	"#8b5cf6",
	"#06b6d4",
	"#ec4899",
	"#14b8a6",
	"#f97316",
	"#6366f1",
	"#84cc16",
	"#d946ef",
	"#0ea5e9",
	"#f43f5e",
	"#10b981",
	"#a855f7",
	"#eab308",
	"#0891b2",
	"#db2777",
	"#7c3aed",
];

const getMarkerColor = (menu: AuthMenuInfoDto) => {
	let colorIndex = 0;
	for (let index = 0; index < menu.menuCode.length; index++) colorIndex += menu.menuCode.charCodeAt(index);
	return markerColors[colorIndex % markerColors.length];
};

const handleMenuClick = (menu: AuthMenuInfoDto) => {
	if (menu.router) {
		router.push(menu.router);
	} else if (menu.link) {
		router.push({
			path: CommonRoute.WebView,
			query: {
				url: encodeURIComponent(menu.link),
				title: encodeURIComponent(menu.menuName),
			},
		});
	}
};

const loadPage = async () => {
	state.searchValue = "";
	state.commonMenuCodes = await employeeApi.queryCommonMenus();
};

onShow(() => {
	loadPage();
});

onPullDownRefresh(async () => {
	try {
		// 删除 HTTP 缓存数据
		Local.removeByPrefix("HTTP_CACHE_");
		// 刷新用户信息
		await userInfoStore.refreshUserInfo();
		// 刷新字典
		await appStore.setDictionary();
		await loadPage();
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
