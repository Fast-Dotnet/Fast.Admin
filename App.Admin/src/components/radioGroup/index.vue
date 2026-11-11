<template>
	<wd-radio-group v-bind="wdRadioGroupProps" v-model="modelValue" @change="({ value }) => emit('change', value)">
		<wd-radio
			v-for="(item, index) in dictionaries"
			:key="index"
			:value="item.value"
			:disabled="item.disabled === false ? undefined : item.disabled"
		>
			{{ item.label }}
		</wd-radio>
	</wd-radio-group>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useProps } from "@fast-china/utils";
import { radioGroupProps } from "@wot-ui/ui/components/wd-radio-group/types";
import { useApp } from "@/stores";

defineOptions({
	name: "RadioGroup",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...radioGroupProps,
	/** 字典名称 */
	name: {
		type: String,
		required: true,
	},
});

const wdRadioGroupProps = useProps(props, radioGroupProps, ["modelValue"]);

const emit = defineEmits<{
	/** 当绑定值变化时触发的事件 */
	change: [value: string | number | boolean];
}>();

/** 当前选中的字典值 */
const modelValue = defineModel<string | number | boolean>({ type: [String, Number, Boolean] });

const appStore = useApp();
// 当前字典中允许显示的选项
const dictionaries = computed(() => (props.name ? appStore.getDictionary(props.name).filter((f) => f.show) : []));
</script>
