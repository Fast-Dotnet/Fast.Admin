<template>
	<view v-if="props.loading" class="fa-loading-page">
		<view class="fa-loading-page__box">
			<view class="box-icon" />
			<view class="box-text">{{ props.text }}</view>
		</view>
	</view>
</template>

<script setup lang="ts">
defineOptions({
	name: "LoadingPage",
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
});
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>
