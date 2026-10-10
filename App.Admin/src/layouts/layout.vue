<template>
	<wd-config-provider
		:custom-class="`fa-layout fa-layout__${theme}`"
		:theme="theme"
		:theme-vars="configStore.layout.themeVars"
		:custom-style="configStore.layout.themeStyle"
	>
		<FaWatermark v-if="state.watermark" />
		<view :class="['fa-main', { 'fa-main__tabBar': state.isTabBar, 'fa-main__page-scroll': state.pageScroll === false }]" :style="mainStyle">
			<slot v-if="state.rendered" />
			<!-- 页脚 -->
			<FaFooter v-if="showFooter" />
			<!-- 底部导航栏 -->
			<wd-tabbar
				v-if="state.isTabBar"
				v-model="userInfoStore.activeTabBar"
				custom-class="fa-tabbar"
				shape="round"
				:active-color="configStore.layout.themeColor"
				inactive-color="var(--wot-text-secondary)"
				:z-index="2001"
				bordered
				fixed
				placeholder
				safe-area-inset-bottom
				@change="({ value }: { value: string }) => router.pushTab(value)"
			>
				<wd-tabbar-item :name="CommonRoute.Home" title="首页">
					<template #icon="{ active }">
						<FaIcon :custom-class="`${active ? 'is-active' : 'is-inactive'}`" name="home" size="18px" />
					</template>
				</wd-tabbar-item>
				<wd-tabbar-item :name="CommonRoute.Workbench" title="工作台">
					<template #icon="{ active }">
						<FaIcon :custom-class="`${active ? 'is-active' : 'is-inactive'}`" name="workbench" size="18px" />
					</template>
				</wd-tabbar-item>
				<wd-tabbar-item :name="CommonRoute.My" title="我的">
					<template #icon="{ active }">
						<FaIcon :custom-class="`${active ? 'is-active' : 'is-inactive'}`" name="my" size="18px" />
					</template>
				</wd-tabbar-item>
			</wd-tabbar>
		</view>
		<!-- 消息通知 -->
		<wd-notify selector="#fast_notify" />
		<!-- 轻提示 -->
		<wd-toast selector="#fast_toast" />
		<!-- 消息弹窗 -->
		<wd-dialog selector="#fast_dialog" />
		<!-- 蒙版层加载 -->
		<FaLoading :loading="!wdHookState.loading?.fullscreen && wdHookState.loading?.state" :text="wdHookState.loading?.text" />
		<!-- 全屏加载 -->
		<FaLoadingPage :loading="wdHookState.loading?.fullscreen && wdHookState.loading?.state" :text="wdHookState.loading?.text" />
		<!-- 遮罩层 -->
		<FaOverlay :visible="wdHookState.overlay?.state" :transparent="wdHookState.overlay?.transparent" />

		<!-- 手机号弹窗 -->
		<!-- #ifdef MP-WEIXIN -->
		<FaPopup ref="authMobilePopupRef" width="80%" :close-on-click-modal="false">
			<view class="fa-auth-mobile">
				<image class="fa-auth-mobile__logo" :src="appStore.logoUrl || defaultLogo" mode="aspectFit" />
				<view class="fa-auth-mobile__title">绑定手机号</view>
				<view class="fa-auth-mobile__description">该微信尚未绑定手机号。授权手机号后，将自动匹配对应管理账号并继续登录。</view>
				<view class="fa-auth-mobile__agreement">
					授权即表示您已阅读并同意
					<text class="fa-auth-mobile__link" @click.stop="router.push(CommonRoute.ServiceAgreement)">《服务协议》</text>
					和
					<text class="fa-auth-mobile__link" @click.stop="router.push(CommonRoute.PrivacyAgreement)">《隐私政策》</text>
				</view>
				<view class="fa-auth-mobile__actions">
					<wd-button type="info" block variant="plain" @click="authMobilePopupRef?.close()">暂不登录</wd-button>
					<wd-button open-type="getPhoneNumber" type="primary" block @getphonenumber="handlePhoneLogin">授权手机号</wd-button>
				</view>
			</view>
		</FaPopup>
		<!-- #endif -->
	</wd-config-provider>
</template>

<script setup lang="ts">
import { onHide, onLoad, onShow } from "@dcloudio/uni-app";
import { type WatchStopHandle, computed, nextTick, onBeforeMount, reactive, ref, watch } from "vue";
import { logger, throttle } from "@fast-china/utils";
import { useRoute, useRouter } from "@wot-ui/router";
import { useDialog } from "@wot-ui/ui/components/wd-dialog";
import { useNotify } from "@wot-ui/ui/components/wd-notify";
import { useToast } from "@wot-ui/ui/components/wd-toast";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { CommonRoute } from "@/common";
import { useMessageBox, wdHookState } from "@/hooks";
import defaultLogo from "@/static/logo.png";
import { useApp, useConfig, useUserInfo } from "@/stores";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "Layout",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const appStore = useApp();
const configStore = useConfig();
const userInfoStore = useUserInfo();
const router = useRouter();

let wdNotifyWatch: WatchStopHandle;
const uNotify = useNotify("#fast_notify");
let wdToastWatch: WatchStopHandle;
const uToast = useToast("#fast_toast");
let wdDialogWatch: WatchStopHandle;
const uDialog = useDialog("#fast_dialog");
let authMobileWatch: WatchStopHandle;

const authMobilePopupRef = ref<FaPopupInstance>();

const state = reactive({
	/** 页面滚动 */
	pageScroll: true,
	/** 显示页脚 */
	footer: true,
	/** 显示水印 */
	watermark: true,
	/** 导航页 */
	isTabBar: false,
	/** 背景颜色 */
	backgroundColor: "var(--wot-filled-bottom)",
	/** 渲染结束 */
	rendered: false,
});

/** 主题 */
const theme = computed(() => (configStore.layout.isDark ? "dark" : "light"));

/** 显示页脚 */
const showFooter = computed(() => configStore.layout.footer && state.footer !== false);

/** 主页面样式 */
const mainStyle = computed(() => {
	const style: Record<string, string> = {};
	let height = "var(--fa-window-height, 100vh)";
	if (showFooter.value) height += " - var(--fa-footer-height, 40px)";
	if (state.isTabBar) height += " - var(--wot-tabbar-height, 55px)";
	style["--main-height"] = `calc(${height})`;
	if (state.backgroundColor) {
		style["background-color"] = state.backgroundColor;
	} else {
		style["background-color"] = "var(--wot-filled-bottom)";
	}
	return style;
});

/** 手机登录 */
const handlePhoneLogin = throttle(({ detail }: { detail: UniHelper.ButtonOnGetphonenumberDetail }) => {
	authMobilePopupRef.value.close(async () => {
		logger.log("GetPhoneNumber", detail);
		const { code } = detail;
		const weChatCode = await userInfoStore.getWeChatCode();
		if (!code || !weChatCode) {
			uToast.warning("授权失败，无法获取您的信息。请重新授权以继续使用我们的服务。");
			return;
		}
		const loginRes = await loginApi.weChatAuthLogin({ weChatCode, code });
		switch (loginRes.status) {
			case LoginStatusEnum.Success:
				userInfoStore.login(loginRes);
				router.replaceAll(CommonRoute.Workbench);
				break;
			case LoginStatusEnum.SelectTenant:
				uni.navigateTo({
					url: CommonRoute.SelectTenant,
					success({ eventChannel }) {
						eventChannel.emit("selectTenantLogin", {
							nickName: loginRes.nickName,
							loginTicket: loginRes.loginTicket,
							tenantList: loginRes.tenantList,
						});
					},
				});
				break;
			default:
				useMessageBox.alert(loginRes.message || "登录失败");
		}
	});
});

/** 消费共享提示请求；页面再次显示时只恢复已经停止的监听。 */
const handleWatch = () => {
	wdNotifyWatch ||= watch(
		() => wdHookState.wdNotify,
		(newValue) => {
			if (!newValue?.type) return;
			if (newValue.type === "closeNotify") uNotify.closeNotify();
			else uNotify.showNotify(newValue.options);
			wdHookState.wdNotify = undefined;
		}
	);

	wdToastWatch ||= watch(
		() => wdHookState.wdToast,
		(newValue) => {
			if (!newValue?.type) return;
			if (newValue.type === "close") uToast.close();
			else uToast[newValue.type](newValue.options);
			wdHookState.wdToast = undefined;
		}
	);

	wdDialogWatch ||= watch(
		() => wdHookState.wdMessageBox,
		(newValue) => {
			if (!newValue?.type) return;
			if (newValue.type === "close") {
				uDialog.close();
			} else {
				uDialog[newValue.type](newValue.options).then(newValue.then).catch(newValue.catch);
			}
			wdHookState.wdMessageBox = undefined;
		}
	);

	// #ifdef MP-WEIXIN
	authMobileWatch ||= watch(
		() => userInfoStore.authMobilePopup,
		(newValue) => {
			if (newValue) authMobilePopupRef.value?.open();
			else if (authMobilePopupRef.value?.visible) authMobilePopupRef.value.close();
		}
	);
	// #endif
};

onLoad(() => {
	handleWatch();
	nextTick(() => {
		state.rendered = true;
	});
});
onShow(handleWatch);
onHide(() => {
	// 隐藏页面停止消费，避免多个页面布局同时响应同一提示请求。
	wdNotifyWatch && wdNotifyWatch();
	wdNotifyWatch = undefined;
	wdToastWatch && wdToastWatch();
	wdToastWatch = undefined;
	wdDialogWatch && wdDialogWatch();
	wdDialogWatch = undefined;

	// #ifdef MP-WEIXIN
	authMobileWatch && authMobileWatch();
	authMobileWatch = undefined;
	// #endif
});
onBeforeMount(() => {
	const { pageScroll, footer, watermark, isTabBar, backgroundColor } = useRoute();
	state.pageScroll = pageScroll ?? true;
	state.footer = footer ?? true;
	state.watermark = watermark ?? true;
	state.isTabBar = isTabBar ?? false;
	state.backgroundColor = backgroundColor ?? "var(--wot-filled-bottom)";
});
</script>

<style scoped lang="scss">
@use "./layout.scss";
</style>
