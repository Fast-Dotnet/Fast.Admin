<template>
	<wd-avatar
		v-bind="wdAvatarProps"
		:custom-class="`fa-avatar ${props.customClass}`"
		:src="state.src"
		@click="emit('click')"
		@error="
			(event: Event) => {
				state.loadError ||= true;
				emit('error', event);
			}
		"
	/>
</template>

<script setup lang="ts">
import { computed, reactive, toRef, watch } from "vue";
import { useProps } from "@fast-china/utils";
import { avatarProps } from "@wot-ui/ui/components/wd-avatar/types";

defineOptions({
	name: "Avatar",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...avatarProps,
	/** 图标名称 */
	icon: {
		type: String,
		default: "user",
	},
	/** src 是否为不带 Data URL 前缀的 PNG Base64 内容 */
	base64: Boolean,
	/** 是否直接使用原始地址，优先于其他图片规格 */
	original: Boolean,
	/** 是否使用 @!normal 图片规格 */
	normal: Boolean,
	/** 是否使用 @!small 图片规格 */
	small: Boolean,
	/** 是否使用 @!thumb 图片规格；未指定规格时也使用缩略图 */
	thumb: Boolean,
});

const wdAvatarProps = useProps(props, avatarProps, ["customClass", "src"]);

const emit = defineEmits<{
	/** 点击事件 */
	click: [];
	/** 当错误发生时触发 */
	error: [event: Event];
}>();

const state = reactive({
	/** 图片加载错误 */
	loadError: false,
	/** 图片地址 */
	src: computed(() => {
		if (!props.src) return undefined;
		if (props.base64) {
			return `data:image/png;base64,${props.src}`;
		} else if (props.original || state.loadError) {
			return props.src;
		} else if (props.normal) {
			return `${props.src}@!normal`;
		} else if (props.small) {
			return `${props.src}@!small`;
		} else if (props.thumb) {
			return `${props.src}@!thumb`;
		} else {
			// 默认使用缩略图
			return `${props.src}@!thumb`;
		}
	}),
});

watch(
	() => props.src,
	() => {
		state.loadError = false;
	}
);

defineExpose({
	/** 处理图片规格后的地址，可能不同于实际显示的占位图地址 */
	src: toRef(state, "src"),
});
</script>
