<template>
	<view class="page">
		<wd-text custom-class="pl20" text="主题样式" size="var(----wot-typography-label-size-main)" />
		<wd-cell-group border>
			<wd-cell title="主题颜色" :value="state.themeColorLabel" is-link @click="state.themeColorVisible = true" />
			<wd-picker v-model="state.themeColor" v-model:visible="state.themeColorVisible" :columns="predefineColorList" />
			<wd-cell title="主题色导航栏" label="开启后导航栏颜色随主题色变换">
				<wd-switch v-model="configStore.layout.autoThemeNavBar" @change="configStore.setTheme(configStore.layout.themeColor)" />
			</wd-cell>
			<wd-cell title="跟随系统" label="开启后自动跟随系统主题">
				<wd-switch v-model="configStore.layout.autoThemMode" @change="configStore.switchAutoThemMode" />
			</wd-cell>
			<wd-cell title="深色模式">
				<wd-switch :disabled="configStore.layout.autoThemMode" v-model="configStore.layout.isDark" @change="configStore.switchDark" />
			</wd-cell>
			<wd-cell title="置灰模式">
				<wd-switch v-model="configStore.layout.isGrey" @change="({ value }) => configStore.switchGreyOrWeak('grey', value)" />
			</wd-cell>
			<wd-cell title="色弱模式">
				<wd-switch v-model="configStore.layout.isWeak" @change="({ value }) => configStore.switchGreyOrWeak('weak', value)" />
			</wd-cell>
		</wd-cell-group>

		<wd-text custom-class="pl20" text="界面显示" size="var(----wot-typography-label-size-main)" />
		<wd-cell-group border>
			<wd-cell title="页脚">
				<wd-switch v-model="configStore.layout.footer" />
			</wd-cell>
		</wd-cell-group>

		<wd-text custom-class="pl20" text="列表配置" size="var(----wot-typography-label-size-main)" />
		<wd-cell-group border>
			<wd-cell title="隐藏图片" label="开启后列表所有图片列需要点击才能查看（减少网络资源消耗）">
				<wd-switch v-model="configStore.tableLayout.hideImage" />
			</wd-cell>
			<wd-cell title="默认搜索时间范围" :value="state.dataSearchRangeLabel" is-link @click="state.dataSearchRangePickerVisible = true" />
			<wd-picker v-model="state.dataSearchRange" v-model:visible="state.dataSearchRangePickerVisible" :columns="dataSearchRangeList" />
		</wd-cell-group>

		<wd-button custom-class="btn__reset" type="danger" @click="configStore.reset">重置</wd-button>
	</view>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue";
import { withDefineType } from "@fast-china/utils";
import { defaultThemeColor, useConfig } from "@/stores";
import type { PickerOption } from "@wot-ui/ui/components/wd-picker-view/types";

definePage({
	name: "SettingGeneral",
	layout: "layout",
	style: {
		navigationBarTitleText: "通用",
	},
});

const configStore = useConfig();

const predefineColorList: PickerOption[] = [
	{
		label: "默认",
		value: defaultThemeColor,
	},
	{ label: "靛蓝", value: "#2A3A93" },
	{ label: "朱红", value: "#EB4537" },
	{ label: "橙色", value: "#FF8225" },
	{ label: "金黄", value: "#FAC230" },
	{ label: "淡紫", value: "#AD6DEF" },
	{ label: "亮蓝色", value: "#4488FE" },
	{ label: "薄荷绿", value: "#55AF7B" },
	{ label: "樱花粉", value: "#FF4191" },
	{ label: "天空蓝", value: "#4286F3" },
];

const dataSearchRangeList: PickerOption[] = [
	{
		label: "近1天",
		value: "Past1D",
	},
	{
		label: "近3天",
		value: "Past3D",
	},
	{
		label: "近1周",
		value: "Past1W",
	},
	{
		label: "近1月",
		value: "Past1M",
	},
	{
		label: "近3月",
		value: "Past3M",
	},
	{
		label: "近6月",
		value: "Past6M",
	},
	{
		label: "近1年",
		value: "Past1Y",
	},
	...(import.meta.env.DEV
		? [
				{
					label: "近3年",
					value: withDefineType<FaTableDataRange>("Past3Y"),
				},
			]
		: []),
];

const state = reactive({
	themeColor: computed({
		get: () => [configStore.layout.themeColor],
		set: (value) => {
			configStore.layout.themeColor = value[0];
			configStore.setTheme(value[0]);
		},
	}),
	themeColorVisible: false,
	themeColorLabel: computed(() => predefineColorList.find((item) => item.value === configStore.layout.themeColor)?.label || ""),
	dataSearchRange: computed({
		get: () => [configStore.tableLayout.dataSearchRange],
		set: (value) => {
			configStore.tableLayout.dataSearchRange = value[0];
		},
	}),
	dataSearchRangePickerVisible: false,
	dataSearchRangeLabel: computed(() => dataSearchRangeList.find((item) => item.value === configStore.tableLayout.dataSearchRange)?.label || ""),
});
</script>

<style scoped lang="scss">
.page {
	padding-top: 20rpx;
	padding-bottom: 20rpx;
}

:deep(.wd-cell-group) {
	--wot-cell-arrow-size: 16px;
	margin-bottom: 20rpx;
}

:deep(.wd-cell__label) {
	font-size: 12px;
}

:deep(.wd-cell__right) {
	flex: unset;
}

:deep(.wd-switch) {
	--wot-switch-size: 40rpx;
}

.btn__reset {
	display: block;
	margin: 30rpx 50rpx;
}
</style>
