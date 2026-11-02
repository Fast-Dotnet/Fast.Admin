<template>
	<view class="page">
		<view class="workbench-header" :style="headerStyle">
			<FaSearchInput v-model="state.searchValue" :filter="false" placeholder="搜索应用名称" />
			<button class="workspace" @click="switchTenantPopupRef?.open()">
				<view class="workspace__name">
					<view class="workspace__logo">
						<FaImage
							width="48rpx"
							height="48rpx"
							:src="userInfoStore.logoUrl || defaultLogo"
							original
							:hide-image="false"
							:enable-preview="false"
							@click="switchTenantPopupRef?.open()"
						/>
					</view>
					<text class="workspace__title">{{ userInfoStore.tenantName }}</text>
				</view>
				<text class="workspace__department">{{ userInfoStore.departmentName || "当前工作空间" }}</text>
				<wd-icon name="arrow-down" size="24rpx" color="var(--wot-text-secondary)" />
			</button>
		</view>

		<view class="workbench-content">
			<view v-if="!state.searchValue.trim()" class="fa-card menu-section">
				<view class="section-header">
					<view class="section-title">
						<view class="section-icon">
							<wd-icon name="star" size="16px" color="var(--wot-primary-6)" />
						</view>
						常用应用
					</view>
					<wd-button type="primary" variant="text" size="small" @click="commonMenuPopupRef?.open(commonMenuList)">编辑</wd-button>
				</view>

				<MenuGrid v-if="commonMenuList.length" :menus="commonMenuList" @select="handleMenuClick" />

				<view v-else class="menu-empty" @click="commonMenuPopupRef?.open(commonMenuList)">点击编辑，添加你的常用应用</view>
			</view>

			<view
				v-for="menu in filteredMenuList"
				:key="menu.menuCode"
				class="fa-card menu-section"
				:class="{ 'is-collapsed': state.collapsedMenuCodes.includes(menu.menuCode) }"
			>
				<button class="section-header section-header--toggle" @click="handleToggleGroup(menu.menuCode)">
					<view class="section-title">
						<view class="section-icon">
							<view class="section-marker" :style="{ backgroundColor: getMarkerColor(menu) }" />
						</view>
						<text>{{ menu.menuName }}</text>
					</view>
					<view class="section-actions">
						<text class="section-count">{{ menu.children.length }} 个应用</text>
						<wd-icon :name="state.collapsedMenuCodes.includes(menu.menuCode) ? 'arrow-down' : 'arrow-up'" size="14px" />
					</view>
				</button>
				<view v-show="!state.collapsedMenuCodes.includes(menu.menuCode)">
					<MenuGrid :menus="menu.children" @select="handleMenuClick" />
				</view>
			</view>
			<wd-empty v-if="!filteredMenuList.length" :tip="state.searchValue.trim() ? '没有找到相关应用' : '暂无可访问的应用'" />
		</view>

		<FaSwitchTenantPopup ref="switchTenantPopupRef" @change="loadPage" />
		<CommonMenuPopup ref="commonMenuPopupRef" :menus="menuList" @ok="loadPage" />
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh, onShareAppMessage, onShareTimeline } from "@dcloudio/uni-app";
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
		navigationStyle: "custom",
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
	collapsedMenuCodes: withDefineType<string[]>([]),
});

const headerStyle = computed(() => {
	const style: Record<string, string> = {};
	// #ifdef MP-WEIXIN
	const { menuButton, windowInfo } = appStore;
	if (menuButton?.width > 0) {
		// 搜索框固定高 40px，与胶囊垂直居中，并预留右侧胶囊空间。
		style["padding-top"] = `${Math.max(menuButton.top + (menuButton.height - 40) / 2, windowInfo.statusBarHeight)}px`;
		style["--workbench-capsule-space"] = `${windowInfo.windowWidth - menuButton.left}px`;
	} else {
		// 胶囊尺寸不可用时，将搜索框放到系统导航区域下方。
		style["padding-top"] = "calc(var(--fa-status-bar-height, 0px) + var(--wot-navbar-height, 44px))";
	}
	// #endif
	return style;
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

// 常见菜单分组对应的颜色错开色系，避免相邻标题连续出现近似颜色。
const markerColors = [
	"#14b8a6",
	"#f43f5e",
	"#3b82f6",
	"#84cc16",
	"#f97316",
	"#a855f7",
	"#22c55e",
	"#64748b",
	"#d946ef",
	"#ef4444",
	"#0ea5e9",
	"#8b5cf6",
	"#b45309",
	"#10b981",
	"#eab308",
	"#ec4899",
	"#6366f1",
	"#0891b2",
	"#f59e0b",
	"#06b6d4",
];

const getMarkerColor = (menu: AuthMenuInfoDto) => {
	let colorIndex = 0;
	for (let index = 0; index < menu.menuCode.length; index++) colorIndex += menu.menuCode.charCodeAt(index);
	return markerColors[colorIndex % markerColors.length];
};

const handleToggleGroup = (menuCode: string) => {
	state.collapsedMenuCodes = state.collapsedMenuCodes.includes(menuCode)
		? state.collapsedMenuCodes.filter((code) => code !== menuCode)
		: [...state.collapsedMenuCodes, menuCode];
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

onLoad(() => {
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
