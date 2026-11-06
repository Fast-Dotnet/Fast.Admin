<template>
	<view class="fa-field">
		<text class="fa-field__title">{{ props.label }}</text>
		<text class="fa-field__content" :style="{ color: props.color, fontSize: props.size ? `${addCssUnit(props.size)}` : undefined }">
			<slot>
				{{ props.value }}
			</slot>
		</text>
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
});
</script>

<style lang="scss" scoped>
.fa-field {
	display: flex;
	align-items: baseline;
	gap: 10rpx;
	font-size: var(--wot-typography-body-size-main);
}

.fa-field__title {
	flex: 0 0 28%;
	color: var(--wot-text-auxiliary);
}

.fa-field__content {
	flex: 1;
	min-width: 0;
	overflow-wrap: anywhere;
	white-space: pre-wrap;
}
</style>
