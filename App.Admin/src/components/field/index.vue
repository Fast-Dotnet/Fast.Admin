<template>
	<view class="fa-field">
		<text class="fa-field__title">{{ props.label }}</text>
		<slot>
			<text
				class="fa-field__content"
				:selectable="props.selectable || props.copyable"
				:user-select="props.selectable || props.copyable"
				:style="{ color: props.color, fontSize: props.size ? `${addCssUnit(props.size)}` : undefined }"
			>
				{{ props.value }}
			</text>
		</slot>
		<button v-if="props.copyable && props.value != null" class="fa-field__copy" hover-class="none" @click.stop="handleCopy">
			<wd-icon name="copy" size="14px" />
			<text>复制</text>
		</button>
	</view>
</template>

<script setup lang="ts">
import { computed } from "vue";
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
		type: [String, Number],
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
	/** 显示复制按钮并允许选中默认文本，复制内容取自 value */
	copyable: Boolean,
});

const handleCopy = () => {
	uni.setClipboardData({
		data: String(props.value),
		fail: () => uni.showToast({ title: "复制失败", icon: "none" }),
	});
};
</script>
