<template>
	<wd-icon
		:custom-class="`fa-icon fa-icon-${props.name} ${props.customClass}`"
		:custom-style="props.customStyle"
		:name="props.name"
		:size="props.size"
		:color="props.color"
		class-prefix="iconfont"
		@click="(event: MouseEvent) => emit('click', event)"
	/>
</template>

<script setup lang="ts">
import { baseProps } from "@wot-ui/ui/common/props";
import "./iconfont.css";

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
	/** 阿里矢量图标名称 */
	name: {
		type: String,
		required: true,
	},
	/** 图标的字体大小 */
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
