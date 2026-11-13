<template>
	<view class="fa-cell-picker">
		<wd-cell
			v-bind="wdCellProps"
			:custom-class="props.customClass"
			:value="props.radio ? undefined : dictionaryLabel"
			:is-link="props.isLink && !props.radio && !formDisabled"
			:clickable="props.clickable && !formDisabled"
			@click="handleClick"
		>
			<template v-if="props.radio && props.clearable && modelValue != null && !formDisabled" #suffix>
				<button class="fa-cell-picker__clear" hover-class="none" @click.stop="handleClear">
					<wd-icon name="close-circle" size="16px" />
				</button>
			</template>
		</wd-cell>
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
			<FaPopup
				v-else
				ref="popupRef"
				custom-class="fa-cell-picker__popup"
				custom-style="--fa-popup-body-padding: 0; min-height: min(240px, 60%); max-height: 60%;"
				:title="props.title"
				:z-index="210"
				position="bottom"
				closable
				@close="visible = false"
			>
				<template v-if="props.clearable" #titlePrefix>
					<wd-button v-if="modelValue != null && !formDisabled" type="primary" variant="text" size="small" @click="handlePopupClear">
						清空
					</wd-button>
				</template>

				<wd-cell-group border>
					<wd-cell
						v-for="item in dictionaries"
						:key="item.value"
						:title="String(item.label)"
						:clickable="!formDisabled && !item.disabled"
						:custom-class="item.value === modelValues[0] ? 'fa-cell-picker__option--selected' : ''"
						:custom-style="item.disabled ? 'opacity: 0.5;' : ''"
						@click="handleSelect(item.value)"
					>
						<template #suffix>
							<wd-icon v-if="item.value === modelValues[0]" name="check" size="16px" color="var(--wot-primary-6)" />
						</template>
					</wd-cell>
				</wd-cell-group>
			</FaPopup>
		</slot>
	</view>
</template>

<script setup lang="ts" generic="TValue extends string | number | boolean">
import { onHide } from "@dcloudio/uni-app";
import { computed, ref } from "vue";
import { randomString, useProps } from "@fast-china/utils";
import { cellProps } from "@wot-ui/ui/components/wd-cell/types";
import { useFormDisabled } from "@wot-ui/ui/composables";
import { useApp } from "@/stores";
import FaPopup from "../popup/index.vue";
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
	/** 已选中时提供清空操作，单选按钮模式显示在单元格右侧，弹窗模式显示在标题左侧 */
	clearable: Boolean,
	/** 是否在结果首部增加值为 null 的“全部”选项 */
	includeAll: Boolean,
});

const wdCellProps = useProps(props, cellProps, ["value", "customClass", "isLink", "clickable"]);

const emit = defineEmits<{
	/** 内容点击事件 */
	click: [];
	/** 当绑定值变化时触发的事件 */
	change: [value: TValue | undefined];
	/** 清空选择时触发 */
	clear: [];
}>();

const popupRef = ref<InstanceType<typeof FaPopup>>();

const visible = ref(false);
const slotState = { visible };

const formDisabled = useFormDisabled(props);

const handleClick = () => {
	if (!props.radio) {
		visible.value = true;
		popupRef.value?.open();
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

const handleClear = () => {
	handleChange();
	emit("clear");
};

const handleSelect = (value: string | number) => {
	handleChange(value);
	popupRef.value?.close();
};

const handlePopupClear = () => {
	handleClear();
	popupRef.value?.close();
};

// #ifdef H5
onHide(() => {
	visible.value = false;
});
// #endif
</script>
