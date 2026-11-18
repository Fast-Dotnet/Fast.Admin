<template>
	<wd-checkbox-group v-bind="wdCheckboxGroupProps" v-model="selectedValues" @change="(_) => emit('change', modelValue)">
		<slot :item-list="dictionaries">
			<wd-checkbox
				v-for="(item, index) in dictionaries"
				:key="index"
				:name="String(item.value)"
				:disabled="item.disabled === false ? undefined : item.disabled"
			>
				{{ item.label }}
			</wd-checkbox>
		</slot>
	</wd-checkbox-group>
</template>

<script setup lang="ts">
import { type PropType, computed } from "vue";
import { omit, useProps } from "@fast-china/utils";
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
	...omit(checkboxGroupProps, ["modelValue"]),
	/** 字典名称；传 false 时由默认插槽提供数字 flags 选项 */
	name: {
		type: [String, Boolean] as PropType<string | false>,
		required: true,
	},
});

const wdCheckboxGroupProps = useProps(props, checkboxGroupProps, ["modelValue"]);

const emit = defineEmits<{
	/** 选项变化时触发，返回十进制 flags 字符串；全部取消时为 undefined */
	change: [value: string | number | boolean];
}>();

/** flags 组合值；选择后回写十进制字符串，全部取消时为 undefined */
const modelValue = defineModel<string | number | boolean>({
	type: [String, Number, Boolean],
	default: undefined,
});

const appStore = useApp();

// 当前字典中允许显示的选项
const dictionaries = computed(() => {
	if (props.name === false || !props.name) return [];
	return appStore.getDictionary(props.name).filter((item) => {
		const flag = BigInt(item.value);
		return item.show && (flag & (flag - 1n)) === 0n;
	});
});
// 字典选项使用字符串标识，手写插槽选项使用数字枚举；只在对外回写时合并为 flags。
const selectedValues = computed({
	get: () => {
		const value = BigInt(modelValue.value || 0n);
		if (props.name !== false) {
			return dictionaries.value.filter((item) => (value & BigInt(item.value)) !== 0n).map((item) => String(item.value));
		}

		const values: number[] = [];
		for (let flag = 1n; flag <= value; flag <<= 1n) {
			if ((value & flag) !== 0n) values.push(Number(flag));
		}
		return values;
	},
	set: (values: (string | number)[]) => {
		modelValue.value = values.length ? values.reduce((value, item) => value | BigInt(item), 0n).toString() : undefined;
	},
});
</script>
