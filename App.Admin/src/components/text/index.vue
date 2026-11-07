<template>
	<wd-text v-bind="wdTextProps" :type="type" :text="dictionary?.label" @click="(evt: MouseEvent) => emit('click', evt)">
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
	/** 值 */
	value: {
		type: [Number, String, Boolean],
		default: undefined,
	},
});

const wdTextProps = useProps(props, textProps, ["type", "text"]);

const emit = defineEmits<{
	/** 标签点击时触发 */
	click: [event: MouseEvent];
}>();

const appStore = useApp();

// 字典
const dictionaries = computed(() => (props.name ? appStore.getDictionary(props.name) : []));
// 字典值
const dictionary = computed(() => (props.value == null ? null : dictionaries.value.find((f) => f.value === props.value)));
const type = computed(() => {
	if (dictionary?.value?.type === "info") return "default";
	if (dictionary?.value?.type === "danger") return "error";

	return dictionary?.value?.type;
});
</script>
