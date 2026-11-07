<template>
	<ElCheckboxGroup v-bind="elCheckboxGroupProps" v-model="selectedValues" @change="(_) => emit('change', modelValue)">
		<template v-if="props.button">
			<ElCheckboxButton
				v-for="(item, index) in dictionaries"
				:key="index"
				:value="String(item.value)"
				:label="item.label"
				:disabled="item.disabled === false ? undefined : item.disabled"
				border
			/>
		</template>
		<template v-else>
			<ElCheckbox
				v-for="(item, index) in dictionaries"
				:key="index"
				:value="String(item.value)"
				:label="item.label"
				:disabled="item.disabled === false ? undefined : item.disabled"
			/>
		</template>
	</ElCheckboxGroup>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { checkboxGroupProps } from "element-plus";
import { useProps } from "@fast-china/utils";
import { useApp } from "@/stores";

defineOptions({
	name: "CheckboxGroup",
});

const props = defineProps({
	// eslint-disable-next-line @typescript-eslint/no-deprecated -- Element Plus 2.x 暂无等价的公开运行时 Props 对象替代。
	...checkboxGroupProps,
	/** 是否使用按钮样式 */
	button: {
		type: Boolean,
		default: false,
	},
	/** 字典名称 */
	name: {
		type: String,
		required: true,
	},
});

// eslint-disable-next-line @typescript-eslint/no-deprecated -- Element Plus 2.x 暂无等价的公开运行时 Props 对象替代。
const elCheckboxGroupProps = useProps(props, checkboxGroupProps, ["modelValue"]);

const emit = defineEmits<{
	/** 当绑定值变化时触发的事件 */
	change: [value: string | number | boolean];
}>();

/** 当前选中的字典值 */
const modelValue = defineModel<string | number | boolean>();

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
