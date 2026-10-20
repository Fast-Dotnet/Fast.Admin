<template>
	<el-form-item
		v-if="state.enabled"
		label="验证码"
		:prop="props.prop"
		:rules="[
			{ required: true, message: '请输入图形验证码', trigger: 'blur' },
			{ pattern: RegExps.ImageCaptchaCode, message: '图形验证码必须为4位字母或数字', trigger: 'blur' },
		]"
	>
		<div class="image-captcha">
			<el-input
				v-model.trim="modelValue"
				autocapitalize="off"
				autocomplete="off"
				:disabled="props.disabled || state.loading || state.loadFailed"
				maxlength="4"
				placeholder="请输入验证码"
				:prefix-icon="PictureRounded"
				:show-word-limit="false"
				spellcheck="false"
			/>
			<button
				class="image-captcha__image"
				type="button"
				aria-label="刷新验证码"
				:disabled="props.disabled || state.loading"
				title="点击刷新验证码"
				@click="refresh"
			>
				<img v-if="state.captchaImage" :src="state.captchaImage" alt="图形验证码" />
				<span v-else>{{ state.loading ? "加载中…" : state.loadFailed ? "加载失败" : "点击重试" }}</span>
			</button>
		</div>
	</el-form-item>
</template>

<script lang="ts" setup>
import { onMounted, reactive } from "vue";
import { PictureRounded } from "@element-plus/icons-vue";
import { RegExps } from "fast-element-plus";
import { loginApi } from "@/api/services/Auth/login";

defineOptions({
	name: "ImageCaptcha",
});

const props = defineProps({
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

onMounted(() => {
	refresh();
});

defineExpose({
	/** 重新获取图形验证码并清空旧答案；请求失败时记录失败状态。 */
	refresh,
});
</script>

<style lang="scss" scoped>
.image-captcha {
	display: grid;
	grid-template-columns: minmax(0, 1fr) 132px;
	align-items: center;
	gap: 8px;
	width: 100%;
	min-width: 0;
}

.image-captcha__image {
	overflow: hidden;
	height: 46px;
	padding: 0;
	color: var(--el-text-color-secondary);
	border: 1px solid var(--el-border-color-lighter);
	border-radius: 8px;
	background: var(--el-fill-color-light);
	cursor: pointer;

	img {
		display: block;
		width: 100%;
		height: 100%;
		object-fit: contain;
	}

	&:disabled {
		cursor: not-allowed;
	}
}

:deep(.el-form-item__label-wrap) {
	align-items: center;
}

@media (max-width: 480px) {
	.image-captcha {
		grid-template-columns: minmax(0, 1fr) 104px;
	}
}
</style>
