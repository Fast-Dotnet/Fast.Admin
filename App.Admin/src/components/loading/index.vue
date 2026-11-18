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
	/** 是否使用父级容器内的绝对定位遮罩；父级需提供定位上下文 */
	mask: Boolean,
});
</script>
