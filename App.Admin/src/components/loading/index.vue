<template>
	<view v-if="props.loading" :class="['fa-loading', { 'fa-loading-mask': props.mask }]">
		<view class="fa-loading__box">
			<view class="box-icon" />
			<view class="box-text">{{ props.text }}</view>
		</view>
	</view>
</template>

<script setup lang="ts">
defineOptions({
	name: "Loading",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 是否显示加载提示 */
	loading: Boolean,
	/** 加载提示文本 */
	text: {
		type: String,
		default: "加载中...",
	},
	/** 是否遮罩层，一般用于组件内部 */
	mask: Boolean,
});
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>
