<template>
	<view class="page">
		<view class="user-card">
			<FaImage width="120rpx" height="120rpx" round original :hide-image="false" :src="userInfoStore.avatar" />
			<view class="user-card__info" @click="router.push('/pages/setting/userInfo/index')">
				<view class="user-card__employeeName">{{ userInfoStore.employeeName }}</view>
				<view class="user-card__employeeNo">{{ userInfoStore.employeeNo }}（{{ userInfoStore.departmentName || "无部门" }}）</view>
				<view class="user-card__nickName">{{ userInfoStore.nickName }}</view>
			</view>
		</view>

		<wd-cell-group border>
			<wd-cell title="个人资料" clickable is-link to="/pages/setting/userInfo/index">
				<template #prefix>
					<FaIcon custom-class="wd-cell__icon" name="idCard" />
				</template>
			</wd-cell>
			<wd-cell title="账号安全" clickable is-link to="/pages/setting/account/index">
				<template #prefix>
					<FaIcon custom-class="wd-cell__icon" name="accountSafe" />
				</template>
			</wd-cell>
			<wd-cell title="工作空间" clickable is-link :value="userInfoStore.tenantName" @click="switchTenantClick">
				<template #prefix>
					<FaIcon custom-class="wd-cell__icon" name="tenant" />
				</template>
			</wd-cell>
		</wd-cell-group>

		<wd-cell-group border>
			<wd-cell title="系统版本" clickable :value="appVersion" />
			<wd-cell title="系统服务商" value="FastDotnet" />
			<wd-cell title="消息通知" clickable is-link @click="useToast.info('敬请期待')" />
			<wd-cell title="通用设置" clickable is-link to="/pages/setting/general/index" />
		</wd-cell-group>

		<wd-cell-group border>
			<wd-cell title="意见反馈" clickable is-link :to="CommonRoute.ComplaintSubmit" />
			<wd-cell title="服务协议" clickable is-link :to="CommonRoute.ServiceAgreement" />
			<wd-cell title="隐私协议" clickable is-link :to="CommonRoute.PrivacyAgreement" />
			<wd-cell title="清除缓存" :value="`${currentSize}/${limitSize}`" clickable @click="handleClearCache" />
		</wd-cell-group>

		<wd-button v-if="userInfoStore.hasUserInfo" custom-class="btn__exit-login" type="danger" block icon="Export" @click="handleLogout">
			退出登录
		</wd-button>

		<FaPopup ref="switchTenantPopupRef" height="70%" position="bottom" :z-index="2001">
			<scroll-view class="tenant-list" scroll-y>
				<view class="tenant-list__content">
					<view v-for="item in state.tenantList" :key="item.userKey" class="page-card tenant-card" @click="handleTenantLogin(item)">
						<image class="tenant-card__image" :src="item.logoUrl" mode="aspectFill" />
						<view class="tenant-card__content">
							<view class="tenant-card__name">{{ item.tenantName }}</view>
							<view class="tenant-card__meta">{{ item.departmentName || "无部门" }} · {{ item.employeeName }}</view>
						</view>
						<FaTag size="small" name="EditionEnum" :value="item.edition" />
						<wd-icon name="arrow-right" size="28rpx" color="var(--wot-text-auxiliary)" />
					</view>
				</view>
			</scroll-view>
		</FaPopup>
	</view>
</template>

<script setup lang="ts">
import { onPageScroll, onPullDownRefresh, onShareAppMessage, onShareTimeline, onShow } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { Local, throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { CommonRoute } from "@/common";
import { useMessageBox, useToast } from "@/hooks";
import defaultLogo from "@/static/logo.png";
import { useApp, useUserInfo } from "@/stores";
import type { LoginTenantOutput } from "@/api/services/Auth/login/models/LoginTenantOutput";
import type { FaPopupInstance } from "@/components";

definePage({
	name: "My",
	layout: "layout",
	isTabBar: true,
	style: {
		navigationBarTitleText: "我的",
		enablePullDownRefresh: true,
	},
});

const appStore = useApp();
const userInfoStore = useUserInfo();
const router = useRouter();

const storageInfo = ref<Partial<UniNamespace.GetStorageInfoSuccess>>({});

const switchTenantPopupRef = ref<FaPopupInstance>();

const state = reactive({
	/** 是否滚动 */
	isScrolled: false,
	tenantList: withDefineType<LoginTenantOutput[]>([]),
});

const appVersion = computed(() => {
	let envName = "";
	switch (appStore.environmentType) {
		case EnvironmentTypeEnum.Production:
			envName = "Prod";
			break;
		case EnvironmentTypeEnum.Development:
			envName = "Dev";
			break;
		case EnvironmentTypeEnum.Test:
			envName = "Test";
			break;
		case EnvironmentTypeEnum.UAT:
			envName = "UAT";
			break;
		case EnvironmentTypeEnum.PreProduction:
			envName = "PreProd";
			break;
		case EnvironmentTypeEnum.GrayDeployment:
			envName = "Gray";
			break;
		case EnvironmentTypeEnum.StressTest:
			envName = "Stress";
			break;
	}
	return `${envName} v${appStore.appBaseInfo.appVersion}`;
});

const currentSize = computed(() => {
	if (storageInfo.value.currentSize >= 1024) {
		const mb = storageInfo.value.currentSize / 1024;
		return `${Number.isInteger(mb) ? mb : mb.toFixed(2)}MB`;
	} else {
		return `${storageInfo.value.currentSize}KB`;
	}
});

const limitSize = computed(() => {
	if (storageInfo.value.limitSize >= 1024) {
		const mb = storageInfo.value.limitSize / 1024;
		return `${Number.isInteger(mb) ? mb : mb.toFixed(2)}MB`;
	} else {
		return `${storageInfo.value.limitSize}KB`;
	}
});

/** 清除缓存 */
const handleClearCache = throttle(() => {
	useMessageBox.confirm("清除缓存后需要重新登录，确定要清除？").then(async () => {
		appStore.clearAppCache();
		await userInfoStore.logout();
		useToast.success("退出登录成功");
	});
});

/** 退出登录 */
const handleLogout = throttle(() => {
	useMessageBox.confirm("确定要退出登录？").then(async () => {
		await userInfoStore.logout();
		useToast.success("退出登录成功");
	});
});

/** 切换租户点击 */
const switchTenantClick = throttle(() => {
	switchTenantPopupRef.value.open(async () => {
		state.tenantList = await loginApi.queryLoginUser();
	});
});

/** 处理租户登录 */
const handleTenantLogin = throttle((data: LoginTenantOutput) => {
	const { userKey } = userInfoStore;
	if (data.userKey === userKey) return;
	switchTenantPopupRef.value
		.close(async () => {
			const loginRes = await loginApi.tryLogin({ userKey: data.userKey });
			if (loginRes.status === LoginStatusEnum.Success) {
				userInfoStore.login(loginRes);
				useToast.success(`切换租户【${data.tenantName}】成功`);
			} else {
				useToast.error(loginRes.message);
			}
		})
		.finally(async () => {
			uni.startPullDownRefresh();
			// 删除 HTTP 缓存数据
			Local.removeByPrefix("HTTP_CACHE_");
			// 刷新用户信息
			await userInfoStore.refreshUserInfo();
			uni.stopPullDownRefresh();
		});
});

onShow(() => {
	storageInfo.value = uni.getStorageInfoSync();
});

onPageScroll(({ scrollTop }) => {
	state.isScrolled = scrollTop > 0;
});

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
