<template>
	<view class="fa-field">
		<text class="fa-field__title">{{ props.label }}</text>
		<slot>
			<text
				class="fa-field__content"
				:selectable="props.selectable"
				:style="{ color: props.color, fontSize: props.size ? `${addCssUnit(props.size)}` : undefined }"
			>
				{{ props.value }}
			</text>
		</slot>
	</view>
</template>

<script setup lang="ts">
import { addCssUnit } from "@fast-china/utils";

defineOptions({
	name: "Field",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 标签 */
	label: {
		type: String,
		required: true,
	},
	/** 值 */
	value: {
		type: [String, Number, Boolean],
		default: undefined,
	},
	/** 内容的字体大小 */
	size: {
		type: [String, Number],
	},
	/** 内容的颜色 */
	color: {
		type: String,
		default: "var(--wot-text-main)",
	},
	/** 是否允许选中内容文本 */
	selectable: Boolean,
});
</script>
