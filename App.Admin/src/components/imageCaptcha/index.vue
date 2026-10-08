<template>
	<wd-form-item v-if="state.enabled" :custom-class="`fa-image-captcha ${props.customClass}`" :prop="props.prop" required>
		<view class="fa-image-captcha__content">
			<wd-input
				v-model="modelValue"
				clearable
				:compact="props.compact"
				prefix-icon="safe"
				:maxlength="4"
				placeholder="请输入图形验证码"
				:disabled="props.disabled || state.loading || state.loadFailed"
			/>
			<button class="fa-image-captcha__image" :disabled="props.disabled || state.loading" @click="refresh">
				<image v-if="state.captchaImage" :src="state.captchaImage" mode="scaleToFill" />
				<text v-else>{{ state.loading ? "加载中…" : state.loadFailed ? "加载失败" : "点击重试" }}</text>
			</button>
		</view>
	</wd-form-item>
</template>

<script setup lang="ts">
import { onMounted, reactive } from "vue";
import { baseProps } from "@wot-ui/ui/common/props";
import { loginApi } from "@/api/services/Auth/login";

defineOptions({
	name: "ImageCaptcha",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...baseProps,
	/**
	 * 表单校验字段名
	 * @defaultValue "captchaCode"
	 */
	prop: {
		type: String,
		default: "captchaCode",
	},
	/** 是否强制启用；false 时由后端登录验证码开关决定 */
	isForce: Boolean,
	/** 业务请求期间禁用输入与手动刷新 */
	disabled: Boolean,
	/** 紧凑模式，移除内边距和背景色 */
	compact: {
		type: Boolean,
		default: false,
	},
});

/** 用户输入的图形验证码 */
const modelValue = defineModel<string>();
/** 当前图形验证码的服务端标识 */
const captchaKey = defineModel<string>("captchaKey");

const state = reactive({
	/** 是否显示图形验证码 */
	enabled: false,
	/** 是否正在获取验证码 */
	loading: false,
	/** 最近一次验证码请求是否失败 */
	loadFailed: false,
	/** 图形验证码图片地址或 Data URL */
	captchaImage: undefined,
});

/** 重新获取图形验证码并清空旧答案。 */
const refresh = async () => {
	state.loading = true;
	state.loadFailed = false;
	captchaKey.value = undefined;
	modelValue.value = undefined;
	state.captchaImage = undefined;
	try {
		const apiRes = await loginApi.getLoginCaptcha(props.isForce).finally(() => {
			state.loading = false;
		});
		state.enabled = props.isForce || apiRes.enabled;
		captchaKey.value = apiRes.captchaKey;
		state.captchaImage = apiRes.captchaImage;
	} catch {
		state.enabled = false;
		state.loadFailed = true;
	}
};

onMounted(refresh);

defineExpose({
	/** 重新获取图形验证码并清空旧答案；请求失败时记录失败状态。 */
	refresh,
});
</script>

<style scoped lang="scss">
.fa-image-captcha__content {
	display: grid;
	grid-template-columns: minmax(0, 1fr) 196rpx;
	align-items: center;
	gap: 16rpx;
	border-bottom: var(--wot-stroke-main) solid var(--wot-divider-main);
}

:deep(.fa-image-captcha__content .wd-input) {
	border: 0;
}

.fa-image-captcha__image {
	display: flex;
	align-items: center;
	justify-content: center;
	width: 100%;
	height: 80rpx;
	padding: 0;
	font-size: 24rpx;
	color: var(--wot-text-auxiliary);
	background-color: var(--wot-filled-bottom);
	border: var(--wot-stroke-light) solid var(--wot-divider-light);
	border-radius: var(--wot-radius-main);
	overflow: hidden;

	&::after {
		border: 0;
	}

	image {
		display: block;
		width: 100%;
		height: 100%;
	}
}
</style>
