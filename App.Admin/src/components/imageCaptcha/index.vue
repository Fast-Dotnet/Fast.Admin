<template>
	<wd-form-item
		v-if="state.enabled"
		:custom-class="`fa-image-captcha ${props.customClass}`"
		:title="props.title"
		:prop="props.prop"
		:layout="props.layout"
		required
	>
		<view class="fa-image-captcha__content" :class="{ 'is-border': props.border }">
			<wd-input
				v-model="modelValue"
				clearable
				:compact="false"
				prefix-icon="safe"
				:maxlength="4"
				placeholder="请输入图形验证码"
				:disabled="props.disabled || state.loading || state.loadFailed"
				@confirm="emit('confirm')"
			/>
			<button class="fa-image-captcha__image" :disabled="props.disabled || state.loading" @click="refresh">
				<image v-if="state.captchaImage" class="fa-image-captcha__picture" :src="state.captchaImage" mode="scaleToFill" />
				<text v-else>{{ state.loading ? "加载中…" : state.loadFailed ? "加载失败" : "点击重试" }}</text>
			</button>
		</view>
	</wd-form-item>
</template>

<script setup lang="ts">
import { type PropType, onMounted, reactive } from "vue";
import { throttle } from "@fast-china/utils";
import { baseProps } from "@wot-ui/ui/common/props";
import { loginApi } from "@/api/services/Auth/login";
import type { CellLayout } from "@wot-ui/ui/components/wd-cell/types";

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
	/** 图形验证码表单项标题 */
	title: String,
	/**
	 * 表单校验字段名
	 * @defaultValue "captchaCode"
	 */
	prop: {
		type: String,
		default: "captchaCode",
	},
	/**
	 * 表单项布局方式
	 * - horizontal：标题与内容左右排列
	 * - vertical：标题与内容上下排列
	 */
	layout: String as PropType<CellLayout>,
	/** 是否强制启用；false 时由后端登录验证码开关决定 */
	isForce: Boolean,
	/** 业务请求期间禁用输入与手动刷新 */
	disabled: Boolean,
	/** 是否显示验证码区域的下边框 */
	border: Boolean,
});

const emit = defineEmits<{
	/** 点击键盘确认键时触发 */
	confirm: [];
}>();

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
const refresh = throttle(async () => {
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
});

onMounted(refresh);

defineExpose({
	/** 重新获取图形验证码并清空旧答案；请求失败时记录失败状态。 */
	refresh,
});
</script>
