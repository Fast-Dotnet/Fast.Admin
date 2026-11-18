<template>
	<view :class="{ 'fa-cell-picker': !props.prop }">
		<wd-form-item
			v-if="props.prop"
			v-bind="wdFormItemProps"
			:value="props.radio ? undefined : dictionaryLabel"
			:placeholder="props.radio ? undefined : props.placeholder"
			:is-link="props.isLink && !props.radio"
			:clickable="props.clickable"
			:disabled="formDisabled"
			@click="handleClick"
		>
			<template v-if="props.radio && props.clearable && modelValue != null && !formDisabled" #default>
				<button class="fa-cell-picker__clear" style="margin-left: auto" hover-class="none" @click.stop="handleClear">
					<wd-icon name="close-circle" size="16px" />
				</button>
			</template>
		</wd-form-item>
		<wd-cell
			v-else
			v-bind="wdCellProps"
			:value="props.radio ? undefined : dictionaryLabel"
			:placeholder="props.radio ? undefined : props.placeholder"
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
				ref="faPopupRef"
				custom-class="fa-cell-picker__popup"
				custom-style="--fa-popup-body-padding: 0;"
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
import { formItemProps } from "@wot-ui/ui/components/wd-form-item/types.ts";
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
	...formItemProps,
	/** 选择入口标题，同时作为内置弹窗标题 */
	title: {
		type: String,
		required: true,
	},
	/** 优先显示的内容；未传时按当前值查找字典标签 */
	value: {
		type: [Number, String],
		default: undefined,
	},
	/** 弹窗选择未选中时的占位文字 */
	placeholder: {
		type: String,
		default: "请选择",
	},
	/** 是否显示右侧箭头；行内单选模式不显示箭头 */
	isLink: {
		type: Boolean,
		default: true,
	},
	/** 标题和内容是否垂直居中 */
	center: {
		type: Boolean,
		default: true,
	},
	/** 内置选择器使用的字典名称；使用自定义插槽时可省略 */
	name: String,
	/** 禁用选择 */
	disabled: Boolean,
	/** 是否在单元格下方直接显示单选按钮 */
	radio: Boolean,
	/** 是否显示清空入口；单选模式位于单元格右侧，弹窗模式位于标题左侧 */
	clearable: Boolean,
	/** 是否在字典选项首部增加“全部”；选择后回写 undefined */
	includeAll: Boolean,
});

const wdCellProps = useProps(props, cellProps, ["value", "placeholder", "isLink", "clickable"]);
const wdFormItemProps = useProps(props, formItemProps, ["value", "placeholder", "isLink", "clickable", "disabled"]);

const emit = defineEmits<{
	/** 点击选择入口时触发 */
	click: [];
	/** 选择、清空或选择“全部”时触发，传递字典项的原始值或 undefined */
	change: [value: TValue | undefined];
	/** 点击清空入口时触发 */
	clear: [];
}>();

const faPopupRef = ref<InstanceType<typeof FaPopup>>();

const visible = ref(false);
const slotState = { visible };

const formDisabled = useFormDisabled(props);

const handleClick = () => {
	if (!props.radio) {
		visible.value = true;
		faPopupRef.value?.open();
	}
	emit("click");
};

/** 选中字典项的原始值；清空或选择“全部”时为 undefined */
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
// 显式传入的显示内容优先，其次根据原始值匹配字典标签。
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
	faPopupRef.value?.close();
};

const handlePopupClear = () => {
	handleClear();
	faPopupRef.value?.close();
};

// #ifdef H5
onHide(() => {
	// 浏览器进入后台时保留弹窗。
	if (document.visibilityState === "hidden") return;

	visible.value = false;
});
// #endif
</script>
