<template>
	<view class="page">
		<view class="page-heading">
			<view class="page-heading__title">{{ state.nickName ? `你好，${state.nickName}` : "欢迎回来" }}</view>
			<view class="page-heading__description">选择一个工作空间，开始今天的工作。</view>
		</view>
		<scroll-view class="tenant-list" scroll-y>
			<view class="tenant-list__content">
				<view v-for="item in state.tenantList" :key="item.userKey" class="fa-card tenant-card" @click="handleTenantLogin(item.userKey)">
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
	</view>
</template>

<script setup lang="ts">
import { onLoad } from "@dcloudio/uni-app";
import { getCurrentInstance, reactive } from "vue";
import { logger, throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { CommonRoute } from "@/common";
import { useMessageBox } from "@/hooks";
import { useUserInfo } from "@/stores";
import type { LoginTenantOutput } from "@/api/services/Auth/login/models/LoginTenantOutput";

definePage({
	name: "SelectTenant",
	layout: "layout",
	noLogin: true,
	authForbidView: true,
	pageScroll: false,
	watermark: false,
	style: { navigationBarTitleText: "选择工作空间" },
});

const { proxy } = getCurrentInstance();

const userInfoStore = useUserInfo();
const router = useRouter();

const state = reactive({
	nickName: withDefineType<string>(),
	loginTicket: withDefineType<string>(),
	tenantList: withDefineType<LoginTenantOutput[]>([]),
});

/** 处理租户登录 */
const handleTenantLogin = throttle(async (userKey: string) => {
	const apiRes = await loginApi.tenantLogin({ userKey, loginTicket: state.loginTicket });
	if (apiRes.status === LoginStatusEnum.Success) {
		userInfoStore.login(apiRes);
		router.replaceAll(CommonRoute.Workbench);
	} else {
		useMessageBox.alert(apiRes.message || "租户登录失败");
	}
});

onLoad(() => {
	(proxy as { getOpenerEventChannel?: () => UniNamespace.EventChannel })
		?.getOpenerEventChannel?.()
		?.once("selectTenantLogin", (data: { nickName: string; loginTicket: string; tenantList: LoginTenantOutput[] }) => {
			logger.log("selectTenantLogin", data);
			state.nickName = data.nickName;
			state.loginTicket = data.loginTicket;
			state.tenantList = data.tenantList;
		});
});
</script>

<style lang="scss" scoped>
.page {
	display: flex;
	flex-direction: column;
	padding: var(--fa-page-padding);
}

.page-heading {
	display: flex;
	flex-direction: column;
	gap: 12rpx;
	padding: 16rpx 4rpx 0;
}

.page-heading__title {
	font-size: var(--wot-typography-title-size-large);
	font-weight: var(--wot-font-weight-semibold);
	line-height: 1.4;
	color: var(--wot-text-main);
	overflow-wrap: anywhere;
}

.page-heading__description {
	font-size: var(--wot-typography-body-size-main);
	line-height: 1.6;
	color: var(--wot-text-secondary);
}

.tenant-list {
	flex: 1;
	min-height: 0;
	height: 0;
	margin-top: 32rpx;
}

.tenant-list__content {
	display: flex;
	flex-direction: column;
	gap: 24rpx;
	padding: 8rpx 4rpx 24rpx;
}

.tenant-card {
	display: flex;
	gap: 18rpx;
	align-items: center;
}

.tenant-card__image {
	flex: none;
	width: 84rpx;
	height: 84rpx;
	border-radius: 18rpx;
	box-shadow: var(--fa-box-shadow-light);
}

.tenant-card__content {
	flex: 1;
	overflow: hidden;
}

.tenant-card__name {
	font-weight: var(--wot-font-weight-semibold);
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.tenant-card__meta {
	margin-top: 6rpx;
	color: var(--wot-text-auxiliary);
	font-size: 24rpx;
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}
</style>
