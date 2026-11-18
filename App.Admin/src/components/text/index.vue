<template>
	<wd-text v-if="props.value != null" v-bind="wdTextProps" :type="type" :text="dictionary?.label" @click="emit('click', $event)">
		<template v-if="$slots.prefix" #prefix>
			<slot name="prefix" />
		</template>
		<template v-if="$slots.suffix" #suffix>
			<slot name="suffix" />
		</template>
	</wd-text>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useProps } from "@fast-china/utils";
import { textProps } from "@wot-ui/ui/components/wd-text/types";
import { useApp } from "@/stores";

defineOptions({
	// eslint-disable-next-line vue/no-reserved-component-names -- 保持现有组件名称与项目引用兼容
	name: "Text",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...textProps,
	/** 字典名称 */
	name: {
		type: String,
		required: true,
	},
	/** 用于严格匹配字典项的值；null 或 undefined 时不渲染 */
	value: {
		type: [Number, String, Boolean],
		default: undefined,
	},
});

const wdTextProps = useProps(props, textProps, ["type", "text"]);

const emit = defineEmits<{
	/** 文本点击时触发 */
	click: [event: MouseEvent];
}>();

const appStore = useApp();

// 指定名称对应的字典项
const dictionaries = computed(() => (props.name ? appStore.getDictionary(props.name) : []));
// 与当前值严格匹配的字典项
const dictionary = computed(() => (props.value == null ? null : dictionaries.value.find((f) => f.value === props.value)));
const type = computed(() => {
	if (dictionary?.value?.type === "info") return "default";
	if (dictionary?.value?.type === "danger") return "error";

	return dictionary?.value?.type;
});
</script>
