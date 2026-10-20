<template>
	<FaDrawer ref="faDrawerRef" :draggable="false" :with-header="false" hide-footer size="300">
		<div class="config-main">
			<el-divider content-position="left">
				<el-icon><Notification /></el-icon>
				布局样式
			</el-divider>
			<div class="config-main__box">
				<div class="layout-mode">
					<el-tooltip effect="dark" placement="top" :show-after="200" content="经典">
						<el-container
							class="layout-mode__Classic"
							:class="{ active: configStore.layout.layoutMode === 'Classic' }"
							@click="configStore.setLayoutMode('Classic')"
						>
							<el-aside />
							<el-container>
								<el-header />
								<el-main />
							</el-container>
						</el-container>
					</el-tooltip>
					<el-tooltip v-if="!isMobile" effect="dark" placement="top" :show-after="200" content="水平">
						<el-container
							class="layout-mode__Horizontal"
							:class="{ active: configStore.layout.layoutMode === 'Horizontal' }"
							@click="configStore.setLayoutMode('Horizontal')"
						>
							<el-header />
							<el-main />
						</el-container>
					</el-tooltip>
					<el-tooltip v-if="!isMobile" effect="dark" placement="top" :show-after="200" content="混合">
						<el-container
							class="layout-mode__Mixed"
							:class="{ active: configStore.layout.layoutMode === 'Mixed' }"
							@click="configStore.setLayoutMode('Mixed')"
						>
							<el-header />
							<el-container>
								<el-aside />
								<el-main />
							</el-container>
						</el-container>
					</el-tooltip>
				</div>
				<div class="box-item">
					<span>切换动画</span>
					<el-select v-model="configStore.layout.mainAnimation" style="width: 120px">
						<el-option v-for="(item, index) in animationList" :key="index" :label="item.label" :value="item.value" />
					</el-select>
				</div>
				<div class="box-item">
					<span>
						跟随分辨率设置
						<el-tooltip effect="dark" content="跟随屏幕分辨率自动切换大小" placement="top">
							<el-icon><QuestionFilled /></el-icon>
						</el-tooltip>
					</span>
					<el-switch v-model="configStore.layout.autoSize" />
				</div>
				<div class="box-item box-item__row">
					<span>布局大小</span>
					<el-radio-group v-model="configStore.layout.layoutSize" :disabled="configStore.layout.autoSize">
						<el-radio-button v-for="(item, index) in layoutSizeList" :key="index" :value="item.value">
							{{ item.label }}
						</el-radio-button>
					</el-radio-group>
				</div>
				<div class="box-item box-item__row">
					<span>页签样式</span>
					<el-radio-group v-model="configStore.layout.navTabStyle">
						<el-radio-button v-for="(item, index) in navTabStyleList" :key="index" :value="item.value">
							{{ item.label }}
						</el-radio-button>
					</el-radio-group>
				</div>
			</div>

			<el-divider content-position="left">
				<el-icon><MagicStick /></el-icon>
				主题样式
			</el-divider>
			<div class="config-main__box">
				<div class="box-item">
					<span>主题颜色</span>
					<ColorPicker v-model="configStore.layout.themeColor" @change="configStore.setTheme" />
				</div>
				<div class="box-item">
					<span>
						跟随系统设置
						<el-tooltip effect="dark" content="跟随系统自动切换浅色/深色模式" placement="top">
							<el-icon><QuestionFilled /></el-icon>
						</el-tooltip>
					</span>
					<el-switch v-model="configStore.layout.autoThemMode" @change="configStore.switchAutoThemMode" />
				</div>
				<div class="box-item">
					<span>深色模式</span>
					<el-switch
						v-model="configStore.layout.isDark"
						:disabled="configStore.layout.autoThemMode"
						:active-action-icon="Sunny"
						:inactive-action-icon="Moon"
						@change="configStore.switchDark"
					/>
				</div>
				<div class="box-item">
					<span>置灰模式</span>
					<el-switch v-model="configStore.layout.isGrey" @change="configStore.switchGreyOrWeak('grey', !!$event)" />
				</div>
				<div class="box-item">
					<span>色弱模式</span>
					<el-switch v-model="configStore.layout.isWeak" @change="configStore.switchGreyOrWeak('weak', !!$event)" />
				</div>
			</div>
			<el-divider content-position="left">
				<el-icon><Grid /></el-icon>
				界面配置
			</el-divider>
			<div class="config-main__box">
				<div class="box-item">
					<span>页签</span>
					<el-switch v-model="configStore.layout.navTab" :active-action-icon="View" :inactive-action-icon="Hide" />
				</div>
				<div class="box-item">
					<span>面包屑</span>
					<el-switch v-model="configStore.layout.breadcrumb" :active-action-icon="View" :inactive-action-icon="Hide" />
				</div>
				<div class="box-item">
					<span>菜单搜索</span>
					<el-switch v-model="configStore.layout.menuSearch" :active-action-icon="View" :inactive-action-icon="Hide" />
				</div>
				<div class="box-item">
					<span>全屏入口</span>
					<el-switch v-model="configStore.layout.screenFull" :active-action-icon="View" :inactive-action-icon="Hide" />
				</div>
				<div class="box-item">
					<span>页脚</span>
					<el-switch v-model="configStore.layout.footer" :active-action-icon="View" :inactive-action-icon="Hide" />
				</div>
				<div class="box-item">
					<span>菜单宽度</span>
					<el-input-number v-model="configStore.layout.menuWidth" :max="240" :min="150" style="width: 80px">
						<template #suffix>
							<span>px</span>
						</template>
					</el-input-number>
				</div>
				<div class="box-item">
					<span>菜单高度</span>
					<el-input-number v-model="configStore.layout.menuHeight" :max="55" :min="40" style="width: 80px">
						<template #suffix>
							<span>px</span>
						</template>
					</el-input-number>
				</div>
			</div>
			<el-button class="button__reset" type="danger" :icon="Refresh" @click="configStore.reset">重置</el-button>
		</div>
	</FaDrawer>
</template>

<script lang="ts" setup>
import { computed, useTemplateRef } from "vue";
import { Grid, Hide, MagicStick, Moon, Notification, QuestionFilled, Refresh, Sunny, View } from "@element-plus/icons-vue";
import { useWindowSize } from "@fast-china/utils";
import { useConfig } from "@/stores";
import type { componentSizes } from "element-plus";
import type { ElSelectorOutput, FaDrawerInstance } from "fast-element-plus";
import type { IAnimationName, INavTabStyle } from "@/stores";

defineOptions({
	name: "LayoutConfig",
});

const faDrawerRef = useTemplateRef<FaDrawerInstance>("faDrawerRef");

const configStore = useConfig();
const windowSize = useWindowSize();

/** 是否移动端（窗口宽度 <= 768） */
const isMobile = computed(() => windowSize.width.value <= 768);

const animationList: Readonly<ElSelectorOutput<IAnimationName>[]> = [
	{
		label: "左到右滑动",
		value: "slide-right",
	},
	{
		label: "右到左滑动",
		value: "slide-left",
	},
	{
		label: "上到下滑动",
		value: "slide-bottom",
	},
	{
		label: "下到上滑动",
		value: "slide-top",
	},
	{
		label: "线性淡入",
		value: "el-fade-in-linear",
	},
	{
		label: "淡入显示",
		value: "el-fade-in",
	},
	{
		label: "中心放大",
		value: "el-zoom-in-center",
	},
	{
		label: "顶部放大",
		value: "el-zoom-in-top",
	},
	{
		label: "底部放大",
		value: "el-zoom-in-bottom",
	},
];

const layoutSizeList: Readonly<ElSelectorOutput<(typeof componentSizes)[number]>[]> = [
	{
		label: "默认",
		value: "default",
	},
	{
		label: "大",
		value: "large",
	},
	{
		label: "小",
		value: "small",
	},
];

const navTabStyleList: Readonly<ElSelectorOutput<INavTabStyle>[]> = [
	{
		label: "灵动",
		value: "Smart",
	},
	{
		label: "卡片",
		value: "Card",
	},
	{
		label: "谷歌",
		value: "Chrome",
	},
];

const open = () => {
	faDrawerRef.value.open();
};

defineExpose({ open });
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>
