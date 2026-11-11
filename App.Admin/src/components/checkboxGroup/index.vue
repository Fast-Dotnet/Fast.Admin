<template>
	<wd-checkbox-group v-bind="wdCheckboxGroupProps" v-model="selectedValues" @change="(_) => emit('change', modelValue)">
		<wd-checkbox
			v-for="(item, index) in dictionaries"
			:key="index"
			:name="String(item.value)"
			:disabled="item.disabled === false ? undefined : item.disabled"
		>
			{{ item.label }}
		</wd-checkbox>
	</wd-checkbox-group>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useProps } from "@fast-china/utils";
import { checkboxGroupProps } from "@wot-ui/ui/components/wd-checkbox-group/types";
import { useApp } from "@/stores";

defineOptions({
	name: "CheckboxGroup",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...checkboxGroupProps,
	/** 字典名称 */
	name: {
		type: String,
		required: true,
	},
});

const wdCheckboxGroupProps = useProps(props, checkboxGroupProps, ["modelValue"]);

const emit = defineEmits<{
	/** 当绑定值变化时触发的事件 */
	change: [value: string | number | boolean];
}>();

/** 当前选中的字典值 */
const modelValue = defineModel<string | number | boolean>({ type: [String, Number, Boolean] });

const appStore = useApp();

// 当前字典中允许显示的选项
const dictionaries = computed(() => {
	if (!props.name) return [];
	return appStore.getDictionary(props.name).filter((item) => {
		const flag = BigInt(item.value);
		return item.show && (flag & (flag - 1n)) === 0n;
	});
});
// 选择的值
const selectedValues = computed({
	get: () => {
		const value = BigInt(modelValue.value || 0n);
		return dictionaries.value.filter((item) => (value & BigInt(item.value)) !== 0n).map((item) => String(item.value));
	},
	set: (values: string[]) => {
		modelValue.value = values.length ? values.reduce((value, item) => value | BigInt(item), 0n).toString() : undefined;
	},
});
</script>
