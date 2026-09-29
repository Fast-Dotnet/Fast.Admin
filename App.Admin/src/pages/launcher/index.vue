<template>
	<view class="page">
		<view class="page__glow page__glow--top" />
		<view class="page__glow page__glow--bottom" />

		<view class="page__brand">
			<view class="page__logo-wrap">
				<view class="page__logo-ring" />
				<image class="page__logo" :src="appStore.logoUrl || defaultLogo" mode="aspectFit" />
			</view>
			<text class="page__title">{{ appStore.appName }}</text>
			<text class="page__subtitle">让管理更简单、更高效</text>
		</view>

		<view class="page__action">
			<view class="page__status">
				<view v-if="state.loading" class="page__loading">
					<view class="page__loading-dot" />
					<view class="page__loading-dot" />
					<view class="page__loading-dot" />
				</view>
				<text>{{ state.statusText }}</text>
			</view>
			<wd-button v-if="state.retryType" custom-class="page__retry" block type="primary" @click="handleRetry">
				{{ state.retryType === "update" ? "重新检测" : "重新连接" }}
			</wd-button>
		</view>

		<FaFooter />
	</view>
</template>

<script setup lang="ts">
import { onLoad } from "@dcloudio/uni-app";
import { reactive } from "vue";
import { logger, withDefineType } from "@fast-china/utils";
import { useRouter } from "uni-mini-router";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { CommonRoute } from "@/common";
import { useUpdate } from "@/hooks";
import defaultLogo from "@/static/logo.png";
import { useApp, useConfig, useUserInfo } from "@/stores";

definePage({
	name: "Launcher",
	layout: "layout",
	footer: false,
	watermark: false,
	pageScroll: false,
	noLogin: true,
	style: { navigationStyle: "custom" },
});

const appStore = useApp();
const configStore = useConfig();
const userInfoStore = useUserInfo();
const router = useRouter();

const state = reactive({
	/** 加载中 */
	loading: true,
	/** 状态文字 */
	statusText: "正在初始化应用",
	// launch 接口异常，则跳过更新
	skipUpdateCheck: false,
	/** 重试类型 */
	retryType: withDefineType<"launch" | "update">(null),
});

const openStartPage = async () => {
	state.statusText = "正在验证登录状态";
	// 判断是否存在 token, userKey
	const { token, userKey } = userInfoStore;
	if (token && userKey) {
		try {
			// 验证登录
			const loginRes = await loginApi.tryLogin({ userKey });
			if (loginRes.status === LoginStatusEnum.Success) {
				userInfoStore.login(loginRes);
				return;
			}
			logger.warn("Launcher", "缓存登录已失效", loginRes);
		} catch (error) {
			logger.error("Launcher", "尝试缓存登录失败", error);
		}

		// 尝试微信自动登录
		// #ifdef MP-WEIXIN
		state.statusText = "正在进行微信授权登录";
		const weChatCode = await userInfoStore.getWeChatCode();
		if (weChatCode) {
			try {
				const loginRes = await loginApi.weChatLogin({ weChatCode });
				if (loginRes.status === LoginStatusEnum.Success) {
					userInfoStore.login(loginRes);
					return;
				}
				logger.warn("Launcher", "微信自动登录失败", loginRes);
			} catch (error) {
				logger.error("Launcher", "微信自动登录失败", error);
			}
		} else {
			logger.warn("Launcher", "微信授权失败，转入手动登录");
		}
		// #endif
	}

	state.statusText = "正在进入应用";
	router.replaceAll(CommonRoute.Login);
};

const checkUpdate = () => {
	if (state.skipUpdateCheck) {
		openStartPage();
	} else {
		// #ifdef H5
		openStartPage();
		// #endif

		// #ifdef MP
		state.statusText = "正在检查版本更新";
		useUpdate
			.checkMiniProgramUpdate()
			.then((hasUpdate) => {
				if (hasUpdate) {
					state.statusText = "新版本已准备好，请完成更新";
				} else {
					openStartPage();
				}
			})
			.catch((error: unknown) => {
				logger.error("Launcher", "小程序更新检测失败", error);
				state.loading = false;
				state.statusText = "更新检测失败";
				state.retryType = "update";
			});
		// #endif

		// #ifdef APP-PLUS
		state.statusText = "正在检查版本更新";
		useUpdate
			.checkAppUpdate(() => {
				return null;
			})
			.then((hasUpdate) => {
				if (hasUpdate) {
					state.statusText = "新版本已准备好，请完成更新";
				} else {
					openStartPage();
				}
			})
			.catch((error: unknown) => {
				logger.error("Launcher", "App更新检测失败", error);
				state.loading = false;
				state.statusText = "更新检测失败";
				state.retryType = "update";
			});
		// #endif
	}
};

const appLaunch = () => {
	state.loading = true;
	state.retryType = null;
	state.skipUpdateCheck = false;
	state.statusText = "正在连接服务";

	appStore
		.launch()
		.then(() => {
			// 设置主题色
			configStore.setTheme(appStore.themeColor);

			state.statusText = "服务连接成功";
			checkUpdate();
		})
		.catch((error: unknown) => {
			logger.error("Launcher", "应用初始化失败", error);

			// launch 接口异常，则跳过更新
			state.skipUpdateCheck = true;

			// 判断是否存在缓存数据
			if (appStore.hasLaunch) {
				state.statusText = "正在使用缓存配置进入应用";
				logger.warn("Launcher", "尝试缓存数据加载应用");
				// 等待3秒进入应用
				setTimeout(() => {
					checkUpdate();
				}, 3000);
			} else {
				state.loading = false;
				state.statusText = appStore.network.networkType === "none" ? "网络连接不可用，请检查网络设置" : "服务暂时无法连接，请稍后重试";
				state.retryType = "launch";
			}
		});
};

const handleRetry = () => {
	const retryType = state.retryType;
	state.loading = true;
	state.retryType = null;

	if (retryType === "update") {
		checkUpdate();
	} else {
		appLaunch();
	}
};

onLoad(appLaunch);
</script>

<style scoped lang="scss">
@use "./index.scss";
</style>
