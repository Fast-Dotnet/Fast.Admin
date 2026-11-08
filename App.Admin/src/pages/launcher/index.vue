<template>
	<view class="page">
		<view class="page__glow page__glow--top" />
		<view class="page__glow page__glow--bottom" />

		<view class="page__brand">
			<view class="page__logo-wrap">
				<view class="page__logo-ring" />
				<image class="page__logo" :src="(state.hasPrivacyConsent && appStore.logoUrl) || defaultLogo" mode="aspectFit" />
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
			<!-- #ifdef APP-PLUS || APP-HARMONY -->
			<wd-button v-if="!state.hasPrivacyConsent" custom-class="page__retry" block type="primary" @click="state.privacyVisible = true">
				查看隐私政策
			</wd-button>
			<!-- #endif -->
			<wd-button v-if="state.retryType" custom-class="page__retry" block type="primary" @click="handleRetry">重新检测</wd-button>
		</view>

		<FaFooter />
		<!-- #ifdef APP-PLUS || APP-HARMONY -->
		<wd-popup v-model="state.privacyVisible" custom-class="privacy-popup" :close-on-click-modal="false" round>
			<view class="privacy-popup__content">
				<view class="privacy-popup__title">服务协议与隐私政策</view>
				<view class="privacy-popup__text">
					欢迎使用 {{ appStore.appName }}！我们重视您的隐私与个人信息保护，请先阅读
					<text class="privacy-popup__link" @click="handleOpenAgreement(CommonRoute.ServiceAgreement)">《服务协议》</text>
					和
					<text class="privacy-popup__link" @click="handleOpenAgreement(CommonRoute.PrivacyAgreement)">《隐私政策》</text>
					。
				</view>
				<view class="privacy-popup__text">
					为提供账号登录、业务办理和运行安全保障，我们会按政策说明处理必要的账号、设备与日志信息。相机、相册、定位等权限仅在使用相应功能时申请，拒绝不会影响其他功能。
				</view>
				<view class="privacy-popup__actions">
					<wd-button type="info" block variant="plain" @click="state.privacyVisible = false">暂不同意</wd-button>
					<wd-button type="primary" block @click="handleAgreePrivacy">同意并继续</wd-button>
				</view>
			</view>
		</wd-popup>
		<!-- #endif -->
	</view>
</template>

<script setup lang="ts">
import { onBackPress, onLoad, onUnload } from "@dcloudio/uni-app";
import { reactive, watch } from "vue";
import { logger, throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { CommonRoute } from "@/common";
import { useToast, useUpdate } from "@/hooks";
import { usePrivacy } from "@/hooks/use-privacy";
import defaultLogo from "@/static/logo.png";
import { useApp, useUserInfo } from "@/stores";

definePage({
	name: "Launcher",
	layout: "layout",
	footer: false,
	watermark: false,
	pageScroll: false,
	noLogin: true,
	style: {
		navigationStyle: "custom",
	},
});

const appStore = useApp();
const userInfoStore = useUserInfo();
const router = useRouter();

const state = reactive({
	/** 加载中 */
	loading: true,
	/** 状态文字 */
	statusText: "正在初始化应用",
	/** 重试类型 */
	retryType: withDefineType<"update">(null),
	/** 是否已获得隐私同意 */
	hasPrivacyConsent: usePrivacy.hasAgreed(),
	/** 隐私协议弹窗 */
	privacyVisible: false,
});

let pageUnloaded = false;
let launchTimer: ReturnType<typeof setTimeout> | undefined;

const openStartPage = async () => {
	state.statusText = "正在验证登录状态";
	let isLoggedIn = false;
	try {
		// 判断是否存在 token, userKey
		const { token, userKey } = userInfoStore;
		if (token && userKey) {
			// 验证登录
			const loginRes = await loginApi.tryLogin({ userKey });
			if (pageUnloaded) return;
			if (loginRes.status === LoginStatusEnum.Success) {
				userInfoStore.login(loginRes);
				isLoggedIn = true;
			}
		}

		// 尝试微信自动登录
		// #ifdef MP-WEIXIN
		if (!isLoggedIn) {
			state.statusText = "正在进行微信授权登录";
			const weChatCode = await userInfoStore.getWeChatCode();
			if (pageUnloaded) return;
			if (weChatCode) {
				const loginRes = await loginApi.weChatLogin({ weChatCode });
				if (pageUnloaded) return;
				if (loginRes.status === LoginStatusEnum.Success) {
					userInfoStore.login(loginRes);
					isLoggedIn = true;
				}
			}
		}
		// #endif
	} catch {
		// 请求层已处理登录失败提示，继续进入登录页。
		logger.warn("Launcher", "自动登录失败，转入登录页");
	}

	if (pageUnloaded) return;
	state.statusText = "正在进入应用";
	await router.replaceAll(isLoggedIn ? CommonRoute.Workbench : CommonRoute.Login);
};

const checkUpdate = async (manual = false) => {
	if (pageUnloaded) return;
	state.statusText = "正在检查版本更新";
	const result = await useUpdate.checkUpdate(manual);
	if (pageUnloaded) return;
	if (result === "restarting") {
		state.statusText = "正在重启应用";
		return;
	}
	if (result === "required" || result === "installing" || result === "failed") {
		state.loading = false;
		state.statusText =
			result === "required"
				? "请完成新版本安装后重新打开应用，未完成可重新检测"
				: result === "installing"
					? "正在安装资源更新，请稍候重新检测"
					: "更新检测失败，请稍后重试";
		state.retryType = "update";
		return;
	}
	await openStartPage();
};

// 启动流程只限制并发，隐私确认或更新重试后无需额外等待冷却窗口。
const start = throttle(async (manual: boolean = false) => {
	if (pageUnloaded) return;
	if (!state.hasPrivacyConsent) {
		state.loading = false;
		state.statusText = "请阅读并同意隐私政策后继续";
		state.privacyVisible = true;
		return;
	}

	state.loading = true;
	state.retryType = null;

	// #ifndef H5
	if (!appStore.hasLaunch) {
		state.statusText = "正在连接服务";
		if (launchTimer === undefined) {
			launchTimer = setTimeout(() => {
				launchTimer = undefined;
				if (pageUnloaded || appStore.hasLaunch) return;
				state.loading = false;
				state.statusText = "应用配置加载超时，请检查网络后重新打开应用";
			}, 10_000);
		}
		return;
	}
	// #endif

	clearTimeout(launchTimer);
	launchTimer = undefined;

	try {
		await checkUpdate(manual);
	} catch (error) {
		if (pageUnloaded) return;
		logger.error("Launcher", "启动流程失败", error);
		state.loading = false;
		state.statusText = "启动失败，请稍后重新打开应用";
	}
}, 0);

// 本地公开协议直接打开，避免在同意前触发路由中的账号初始化。
const handleOpenAgreement = (url: string): void => {
	uni.navigateTo({
		url,
		fail(error) {
			logger.error("Privacy", "打开协议失败", error);
			useToast.error("协议打开失败，请重试");
		},
	});
};

const handleAgreePrivacy = () => {
	if (state.hasPrivacyConsent || pageUnloaded) return;
	try {
		usePrivacy.agree();
		state.hasPrivacyConsent = true;
		state.privacyVisible = false;
		start();
	} catch (error: unknown) {
		logger.error("Privacy", "保存隐私同意记录失败", error);
		state.statusText = "隐私同意记录保存失败，请重试";
	}
};

const handleRetry = () => {
	if (state.retryType) start(true);
};

// #ifndef H5
watch(
	() => appStore.hasLaunch,
	(newValue) => {
		if (newValue) start();
	},
	{ immediate: true, flush: "sync" }
);
// #endif

onLoad(() => {
	start();
});

onUnload(() => {
	pageUnloaded = true;
	clearTimeout(launchTimer);
	launchTimer = undefined;
});

// 返回键不能绕过隐私确认或强制更新。
onBackPress(() => true);
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>
