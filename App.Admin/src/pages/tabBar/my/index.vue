<template>
	<view class="page">
		<view class="user-card">
			<FaImage width="120rpx" height="120rpx" round original :hide-image="false" :src="userInfoStore.avatar" />
			<view
				class="user-card__info"
				@click="!userInfoStore.isSuperAdmin && !userInfoStore.isAdmin && router.push('/pages/setting/employee/index')"
			>
				<view class="user-card__employeeName">{{ userInfoStore.employeeName }}</view>
				<view class="user-card__employeeNo">{{ userInfoStore.employeeNo }}（{{ userInfoStore.departmentName || "无部门" }}）</view>
				<view class="user-card__nickName">{{ userInfoStore.nickName }}</view>
			</view>
		</view>

		<wd-cell-group center border>
			<wd-cell
				v-if="!userInfoStore.isSuperAdmin && !userInfoStore.isAdmin"
				title="个人资料"
				clickable
				is-link
				to="/pages/setting/employee/index"
			>
				<template #prefix>
					<FaIcon custom-class="wd-cell__icon" name="idCard" />
				</template>
			</wd-cell>
			<wd-cell title="账号安全" clickable is-link to="/pages/setting/account/index">
				<template #prefix>
					<FaIcon custom-class="wd-cell__icon" name="accountSafe" />
				</template>
			</wd-cell>
			<wd-cell title="工作空间" clickable is-link :value="userInfoStore.tenantName" @click="switchTenantPopupRef?.open()">
				<template #prefix>
					<FaIcon custom-class="wd-cell__icon" name="tenant" />
				</template>
			</wd-cell>
		</wd-cell-group>

		<wd-cell-group center border>
			<wd-cell title="系统版本" clickable :value="state.checkingUpdate ? '正在检查更新…' : appVersion" @click="handleCheckUpdate" />
			<wd-cell title="系统服务商" value="FastDotnet" />
			<wd-cell title="消息通知" clickable is-link @click="useToast.info('敬请期待')" />
			<wd-cell title="通用设置" clickable is-link to="/pages/setting/general/index" />
		</wd-cell-group>

		<wd-cell-group center border>
			<wd-cell title="意见反馈" clickable is-link :to="CommonRoute.ComplaintSubmit" />
			<wd-cell title="服务协议" clickable is-link :to="CommonRoute.ServiceAgreement" />
			<wd-cell title="隐私协议" clickable is-link :to="CommonRoute.PrivacyAgreement" />
			<wd-cell title="清除缓存" :value="storageSize" clickable @click="handleClearCache" />
		</wd-cell-group>

		<wd-button v-if="userInfoStore.hasUserInfo" custom-class="btn__exit-login" type="danger" block icon="Export" @click="handleLogout">
			退出登录
		</wd-button>

		<FaSwitchTenantPopup ref="switchTenantPopupRef" />
	</view>
</template>

<script setup lang="ts">
import { onPageScroll, onPullDownRefresh, onShareAppMessage, onShareTimeline, onShow } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { Local, logger, throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { Decimal } from "decimal.js";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import { CommonRoute } from "@/common";
import { useMessageBox, useToast, useUpdate } from "@/hooks";
import defaultLogo from "@/static/logo.png";
import { useApp, useUserInfo } from "@/stores";
import type { FaSwitchTenantPopupInstance } from "@/components/index.ts";

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

const switchTenantPopupRef = ref<FaSwitchTenantPopupInstance>();

const state = reactive({
	checkingUpdate: false,
	/** 是否滚动 */
	isScrolled: false,
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
	return `${envName} v${appStore.appVersion}`;
});

const handleCheckUpdate = throttle(async () => {
	state.checkingUpdate = true;
	try {
		const result = await useUpdate.checkUpdate(true);
		if (result === "required") {
			await router.replaceAll(CommonRoute.Launcher);
		}
	} catch (error) {
		// 常规失败由更新入口处理，这里仅记录未预期异常。
		logger.error("My", "手动检查更新失败", error);
	} finally {
		state.checkingUpdate = false;
	}
});

const formatStorageSize = (size: number) => {
	const kb = new Decimal(size);
	if (kb.gte(1024)) {
		return `${kb.div(1024).toDecimalPlaces(2).toFixed()}MB`;
	}
	return `${kb.toDecimalPlaces(2).toFixed()}KB`;
};

const storageSize = computed(() => {
	const { currentSize = 0, limitSize } = storageInfo.value;
	const current = formatStorageSize(currentSize);
	// H5 使用 Number.MAX_VALUE 占位，未提供实际容量上限时只展示已用容量。
	if (!limitSize || limitSize === Number.MAX_VALUE) return current;
	return `${current}/${formatStorageSize(limitSize)}`;
});

/** 清除缓存 */
const handleClearCache = throttle(() => {
	useMessageBox.confirm("清除缓存后需要重新登录，确定要清除？", {
		async beforeConfirm() {
			appStore.clearAppCache();
			await userInfoStore.logout();
			useToast.success("退出登录成功");
		},
	});
});

/** 退出登录 */
const handleLogout = throttle(() => {
	useMessageBox.confirm("确定要退出登录？", {
		async beforeConfirm() {
			await userInfoStore.logout();
			useToast.success("退出登录成功");
		},
	});
});

onShow(() => {
	storageInfo.value = uni.getStorageInfoSync();
});

onPageScroll(({ scrollTop }) => {
	state.isScrolled = scrollTop > 0;
});

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
