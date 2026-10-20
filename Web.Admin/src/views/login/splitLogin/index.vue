<template>
	<div class="split-login" :style="{ '--login-bg': props.background }">
		<slot name="help" />
		<section class="split-login__panel">
			<div aria-hidden="true" class="panel__decoration">
				<div class="deco-grid"></div>
				<div class="deco-glow deco-glow--top"></div>
				<div class="deco-glow deco-glow--bottom"></div>
				<div class="deco-orbit"><span></span></div>
			</div>
			<div class="panel__content">
				<div class="panel__logo">
					<img class="logo-img" :src="logoImage" :alt="appStore.appName" />
					<span class="logo-text">{{ appStore.appName }}</span>
				</div>
				<div class="panel__hero">
					<div class="panel__eyebrow"><span></span>FAST ADMIN PLATFORM</div>
					<h2>让复杂的管理<br /><strong>变得简单清晰</strong></h2>
					<p>一体化企业管理平台，帮助团队更高效地连接业务、数据与协作。</p>
					<div class="panel__features">
						<span><i></i>统一权限</span>
						<span><i></i>多租户架构</span>
						<span><i></i>灵活配置</span>
					</div>
				</div>
				<div class="panel__footnote"><span></span>安全 · 稳定 · 可扩展</div>
			</div>
		</section>

		<section class="split-login__form-wrap">
			<div class="form-stage">
				<div class="form-card">
					<div aria-hidden="true" class="form-card__accent"></div>
					<LoginForm :form-rules="props.formRules" variant="split" />
				</div>
			</div>
			<div class="form-footer" :style="{ '--login-footer-height': addCssUnit(props.footerHeight) }">
				<Footer />
			</div>
		</section>
	</div>
</template>

<script lang="ts" setup>
import { addCssUnit } from "@fast-china/utils";
import logoImage from "@/assets/logo.png";
import { useApp } from "@/stores";
import LoginForm from "../components/loginForm.vue";
import type { FormRules } from "element-plus";

defineOptions({
	name: "SplitLogin",
});

const props = defineProps<{
	/** 页面主题背景 */
	background?: string;
	/** 页脚高度*/
	footerHeight?: number;
	/** 登录表单校验规则 */
	formRules?: FormRules;
}>();

const appStore = useApp();
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>
