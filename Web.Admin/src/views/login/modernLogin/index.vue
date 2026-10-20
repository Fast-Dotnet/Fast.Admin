<template>
	<el-container class="login-page modern-login-page" :style="{ '--login-theme-gradient': props.background }">
		<slot name="help" />
		<div class="login-grid"></div>
		<div class="modern-beam modern-beam--one"></div>
		<div class="modern-beam modern-beam--two"></div>
		<div class="login-orb modern-orb modern-orb--one"></div>
		<div class="login-orb modern-orb modern-orb--two"></div>

		<el-main>
			<div class="modern-shell">
				<section class="modern-brand">
					<div class="brand-lockup">
						<img :src="logoImage" :alt="appStore.appName" />
						<span class="brand-lockup__text">
							<strong>{{ appStore.appName }}</strong>
							<small>DIGITAL OPERATIONS</small>
						</span>
					</div>

					<div class="modern-brand__copy">
						<div class="modern-kicker"><span>●</span> System online · 99.99%</div>
						<h1>
							掌控每一刻
							<em>业务脉动</em>
						</h1>
						<p>数据、协作与决策在同一空间自然流动，让组织始终保持敏捷。</p>
					</div>

					<div class="insight-window" aria-hidden="true">
						<div class="insight-window__bar">
							<span></span><span></span><span></span>
							<small>Realtime overview</small>
						</div>
						<div class="insight-window__content">
							<div class="insight-main">
								<div class="insight-main__head">
									<span>业务趋势</span>
									<el-icon><TrendCharts /></el-icon>
								</div>
								<strong>+28.6%</strong>
								<div class="chart-bars">
									<i v-for="height in chartBars" :key="height" :style="{ height: `${height}%` }"></i>
								</div>
							</div>
							<div class="insight-stat insight-stat--violet">
								<el-icon><DataAnalysis /></el-icon>
								<small>今日处理</small>
								<strong>2,486</strong>
							</div>
							<div class="insight-stat insight-stat--cyan">
								<el-icon><Connection /></el-icon>
								<small>协同效率</small>
								<strong>94.8%</strong>
							</div>
						</div>
						<div class="insight-window__scan"></div>
					</div>
				</section>

				<section class="modern-form-card">
					<div class="modern-form-card__glow"></div>
					<LoginForm variant="modern" :form-rules="props.formRules" />
				</section>
			</div>
		</el-main>

		<el-footer :style="{ '--el-footer-height': addCssUnit(props.footerHeight) }">
			<Footer />
		</el-footer>
	</el-container>
</template>

<script lang="ts" setup>
import { Connection, DataAnalysis, TrendCharts } from "@element-plus/icons-vue";
import { addCssUnit } from "@fast-china/utils";
import logoImage from "@/assets/logo.png";
import { useApp } from "@/stores";
import LoginForm from "../components/loginForm.vue";
import type { FormRules } from "element-plus";

defineOptions({
	name: "ModernLogin",
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
const chartBars = [32, 48, 42, 64, 58, 78, 70, 92, 84, 100];
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>
