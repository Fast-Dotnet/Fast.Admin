<template>
	<wd-icon
		:custom-class="`fa-icon fa-icon-${props.name} ${props.customClass}`"
		:custom-style="props.customStyle"
		:name="props.name"
		:size="props.size"
		:color="props.color"
		class-prefix="iconfont"
		@click="emit('click', $event)"
	/>
</template>

<script setup lang="ts">
import { baseProps } from "@wot-ui/ui/common/props";

defineOptions({
	name: "Icon",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...baseProps,
	/** 项目 iconfont 中的图标名称，不带 fa-icon- 前缀 */
	name: {
		type: String,
		required: true,
	},
	/** 图标字号，数字按 px 处理 */
	size: {
		type: [String, Number],
	},
	/** 图标的颜色 */
	color: String,
});

const emit = defineEmits<{
	/** 点击事件 */
	click: [event: MouseEvent];
}>();
</script>

<style lang="scss" scoped>
@use "./iconfont.scss";
</style>
