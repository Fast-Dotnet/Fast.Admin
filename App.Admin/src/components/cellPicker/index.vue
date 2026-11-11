<template>
	<view class="fa-cell-picker">
		<wd-cell
			v-bind="wdCellProps"
			:custom-class="`fa-cell-picker__cell ${props.customClass}`"
			:value="props.radio ? undefined : dictionaryLabel"
			:is-link="props.isLink && !props.radio && !formDisabled"
			:clickable="props.clickable && !formDisabled"
			@click="handleClick"
		/>
		<slot :visible="slotState.visible">
			<wd-radio-group
				v-if="props.radio"
				:model-value="modelValues[0] ?? (props.includeAll ? allValue : undefined)"
				direction="horizontal"
				type="button"
				@change="({ value }) => handleChange(value)"
			>
				<wd-radio v-for="item in dictionaries" :key="item.value" :value="item.value" :disabled="formDisabled || item.disabled">
					{{ item.label }}
				</wd-radio>
			</wd-radio-group>
			<wd-picker
				v-else
				:model-value="modelValues"
				v-model:visible="visible"
				:columns="dictionaries"
				@confirm="({ value }) => handleChange(value[0])"
			/>
		</slot>
	</view>
</template>

<script setup lang="ts" generic="TValue extends string | number | boolean">
import { computed, ref } from "vue";
import { randomString, useProps } from "@fast-china/utils";
import { cellProps } from "@wot-ui/ui/components/wd-cell/types";
import { useFormDisabled } from "@wot-ui/ui/composables";
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
	/** 是否垂直居中 */
	center: {
		type: Boolean,
		default: true,
	},
	/** 字典名称 */
	name: String,
	/** 禁用选择 */
	disabled: Boolean,
	/** 在 Cell 下方直接显示单选按钮 */
	radio: Boolean,
	/** 是否在结果首部增加值为 null 的“全部”选项 */
	includeAll: Boolean,
});

const wdCellProps = useProps(props, cellProps, ["value", "customClass", "isLink", "clickable"]);

const emit = defineEmits<{
	/** 内容点击事件 */
	click: [];
	/** 当绑定值变化时触发的事件 */
	change: [value: string | number];
}>();

const visible = ref(false);
const slotState = { visible };

const formDisabled = useFormDisabled(props);

const handleClick = () => {
	if (!props.radio) {
		visible.value = true;
	}
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
	() => props.value ?? dictionaries.value.find((item) => item.rawValue === (modelValue.value === null ? allValue : modelValue.value))?.label
);

const handleChange = (value?: string | number) => {
	const selectedValue = dictionaries.value.find((item) => item.value === value)?.rawValue;
	modelValue.value = selectedValue;
	emit("change", selectedValue);
};
</script>

<style lang="scss" scoped>
.fa-cell-picker {
	border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
}

:deep(.fa-cell-picker__cell) {
	.wd-cell__left {
		flex: var(--fa-cell-left-flex, 0 0 28%);
	}

	.wd-cell__arrow-right {
		--wot-cell-arrow-margin-left: 0;
	}
}
</style>
