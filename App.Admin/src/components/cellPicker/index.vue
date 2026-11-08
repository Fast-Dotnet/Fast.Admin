<template>
	<view class="fa-cell-picker">
		<wd-cell v-bind="wdCellProps" :value="dictionaryLabel" @click="handleClick" />
		<slot :visible="slotState.visible" :label="slotState.label">
			<wd-picker :model-value="modelValues" v-model:visible="state.visible" :columns="dictionaries" @confirm="handleConfirm" />
		</slot>
	</view>
</template>

<script setup lang="ts" generic="TValue extends string | number | boolean">
import { computed, reactive, toRefs } from "vue";
import { randomString, useProps, withDefineType } from "@fast-china/utils";
import { cellProps } from "@wot-ui/ui/components/wd-cell/types";
import { useApp } from "@/stores";
import type { PickerOption } from "@wot-ui/ui/components/wd-picker-view/types";

defineOptions({
	name: "CellPicker",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...cellProps,
	/** 标题 */
	title: {
		type: String,
		required: true,
	},
	/** 右侧内容 */
	value: {
		type: [Number, String],
		default: undefined,
	},
	/** 是否展示右侧箭头并开启点击反馈 */
	isLink: {
		type: Boolean,
		default: true,
	},
	/** 字典名称 */
	name: String,
	/** 是否在结果首部增加值为 null 的“全部”选项 */
	includeAll: Boolean,
});

const wdCellProps = useProps(props, cellProps, ["value"]);

const emit = defineEmits<{
	/** 内容点击事件 */
	click: [];
	/** 当绑定值变化时触发的事件 */
	change: [value: string | number];
}>();

const state = reactive({
	visible: false,
	label: withDefineType<string>(),
});
const slotState = toRefs(state);

const handleClick = () => {
	state.visible = true;
	emit("click");
};

const modelValue = defineModel<TValue>();
const allValue = randomString(6);
const modelValues = computed(() => (modelValue.value == null ? [] : [`${typeof modelValue.value}:${modelValue.value}`]));

const appStore = useApp();

const dictionaries = computed<PickerOption[]>(() => {
	if (!props.name) return [];

	const items = appStore.getDictionary(props.name) ?? [];
	const options = items.map((item) => ({
		label: item.label,
		disabled: item.disabled,
		value: `${typeof item.value}:${item.value}`,
		rawValue: item.value,
	}));

	return props.includeAll ? [{ label: "全部", value: allValue, rawValue: undefined }, ...options] : options;
});
// 字典值
const dictionaryLabel = computed(
	() =>
		props.value ??
		state.label ??
		dictionaries.value.find((item) => item.rawValue === (modelValue.value === null ? allValue : modelValue.value))?.label
);

const handleConfirm = ({ value }: { value: (string | number)[] }) => {
	const selectedValue = dictionaries.value.find((item) => item.value === value[0])?.rawValue;
	modelValue.value = selectedValue;
	emit("change", selectedValue);
};
</script>

<style lang="scss" scoped>
.fa-cell-picker {
	border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
}

.wd-cell {
	--wot-cell-arrow-margin-left: 0;
	--wot-cell-bg: none;
}

:deep(.wd-cell__left) {
	flex: 0 0 28%;
}

:deep(.wd-cell__arrow-right) {
	--wot-cell-arrow-margin-left: 0;
}
</style>
