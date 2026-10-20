<template>
	<el-icon
		class="screen-full fa__hover__twinkle"
		role="button"
		tabindex="0"
		:title="isFullscreen ? '退出全屏' : '全屏'"
		:aria-label="isFullscreen ? '退出全屏' : '全屏'"
		@keydown.enter.prevent="handleScreenFullClick"
		@keydown.space.prevent="handleScreenFullClick"
		@click="handleScreenFullClick"
	>
		<FullScreenExit v-if="isFullscreen" />
		<FullScreen v-else />
	</el-icon>
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue";
import { FullScreen, FullScreenExit } from "@fast-element-plus/icons-vue";
import { useScreenFull } from "fast-element-plus";

defineOptions({
	name: "ScreenFull",
});

const isFullscreen = computed(() => useScreenFull.isFullscreen());

const handleScreenFullClick = () => {
	useScreenFull.toggle();
};

onMounted(() => {
	useScreenFull.init();
});
</script>

<style lang="scss" scoped>
.screen-full {
	width: 100%;
	height: 100%;
	font-size: var(--el-font-size-large);
	cursor: pointer;
}

html.small {
	.screen-full {
		font-size: var(--el-font-size-medium);
	}
}
</style>
