<template>
	<el-container class="login-page classic-login-page" :style="{ '--login-theme-gradient': props.background }">
		<slot name="help" />
		<div class="login-grid"></div>
		<div class="login-orb classic-orb classic-orb--one"></div>
		<div class="login-orb classic-orb classic-orb--two"></div>

		<el-main>
			<div class="classic-shell">
				<section class="classic-brand">
					<div class="brand-lockup">
						<img :src="logoImage" :alt="appStore.appName" />
						<span class="brand-lockup__text">
							<strong>{{ appStore.appName }}</strong>
							<small>FAST ADMIN CONSOLE</small>
						</span>
					</div>

					<div class="classic-brand__copy">
						<span class="classic-brand__badge"><i></i> Enterprise Workspace</span>
						<h1>
							让复杂管理
							<span>清晰而高效</span>
						</h1>
						<p>统一连接组织、流程与数据，为每一次管理决策提供可靠支撑。</p>
					</div>

					<div class="classic-visual" aria-hidden="true">
						<div class="classic-visual__halo"></div>
						<div class="classic-console">
							<header class="classic-console__header">
								<span class="console-window"><i></i><i></i><i></i></span>
								<strong>业务工作台</strong>
								<em><i></i>实时</em>
							</header>
							<div class="classic-console__body">
								<div class="console-overview">
									<div>
										<span>运行概览</span>
										<strong>84<small>%</small></strong>
										<p><i></i>核心业务运行平稳</p>
									</div>
									<div class="console-ring"><span></span></div>
								</div>
								<div class="console-chart">
									<div><span>协作趋势</span><small>近 7 日</small></div>
									<p><i></i><i></i><i></i><i></i><i></i><i></i><i></i></p>
								</div>
								<div class="console-metrics">
									<span><small>待办任务</small><strong>24</strong></span>
									<span><small>活跃流程</small><strong>18</strong></span>
									<span><small>组织协同</small><strong>96%</strong></span>
								</div>
							</div>
						</div>
					</div>

					<div class="classic-features">
						<div>
							<el-icon><CircleCheckFilled /></el-icon><span>稳定可靠</span>
						</div>
						<div>
							<el-icon><Connection /></el-icon><span>多端协同</span>
						</div>
						<div>
							<el-icon><TrendCharts /></el-icon><span>数据驱动</span>
						</div>
					</div>
				</section>

				<section class="classic-form">
					<LoginForm variant="classic" :form-rules="props.formRules" />
				</section>
			</div>
		</el-main>

		<el-footer :style="{ '--el-footer-height': addCssUnit(props.footerHeight) }">
			<Footer />
		</el-footer>
	</el-container>
</template>

<script lang="ts" setup>
import { CircleCheckFilled, Connection, TrendCharts } from "@element-plus/icons-vue";
import { addCssUnit } from "@fast-china/utils";
import logoImage from "@/assets/logo.png";
import { useApp } from "@/stores";
import LoginForm from "../components/loginForm.vue";
import type { FormRules } from "element-plus";

defineOptions({
	name: "ClassicLogin",
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
