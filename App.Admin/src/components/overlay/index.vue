<template>
	<view v-if="props.visible" class="fa-overlay" :style="{ backgroundColor: `rgba(0, 0, 0, ${props.transparent})` }" />
</template>

<script setup lang="ts">
defineOptions({
	name: "Overlay",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 是否显示遮罩 */
	visible: Boolean,
	/** 黑色遮罩的 alpha 值，0 为全透明但仍拦截交互，1 为不透明 */
	transparent: {
		type: Number,
		default: 0,
	},
});
</script>

<style scoped lang="scss">
.fa-overlay {
	width: 100%;
	height: 100%;
	position: fixed;
	top: 0;
	left: 0;
	z-index: 2001;
	pointer-events: auto;
	cursor: not-allowed;
}
</style>
