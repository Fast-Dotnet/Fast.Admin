<template>
	<el-container class="login-page simple-login-page" :style="{ '--login-theme-gradient': props.background }">
		<slot name="help" />
		<div class="simple-lines" aria-hidden="true"><i></i><i></i><i></i></div>
		<div class="login-orb simple-orb simple-orb--one"></div>
		<div class="login-orb simple-orb simple-orb--two"></div>

		<el-main>
			<div class="simple-shell">
				<header class="simple-brand">
					<div class="simple-brand__mark">
						<img :src="logoImage" :alt="appStore.appName" />
					</div>
					<div class="simple-brand__copy">
						<strong>{{ appStore.appName }}</strong>
						<small>让管理回归简单</small>
					</div>
				</header>

				<section class="simple-form-card">
					<div class="simple-form-card__accent"></div>
					<LoginForm variant="simple" :form-rules="props.formRules" />
				</section>

				<div class="simple-trust">
					<span>
						<el-icon><Lock /></el-icon>
						<span>数据安全保护</span>
					</span>
					<i></i>
					<span>
						<el-icon><Cloudy /></el-icon>
						<span>私有化部署</span>
					</span>
				</div>
			</div>
		</el-main>

		<el-footer :style="{ '--el-footer-height': addCssUnit(props.footerHeight) }">
			<Footer />
		</el-footer>
	</el-container>
</template>

<script lang="ts" setup>
import { Cloudy, Lock } from "@element-plus/icons-vue";
import { addCssUnit } from "@fast-china/utils";
import logoImage from "@/assets/logo.png";
import { useApp } from "@/stores";
import LoginForm from "../components/loginForm.vue";
import type { FormRules } from "element-plus";

defineOptions({
	name: "SimpleLogin",
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
