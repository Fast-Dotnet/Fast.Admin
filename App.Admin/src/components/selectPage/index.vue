<template>
	<FaPopup
		ref="faPopupRef"
		custom-class="fa-select-page"
		custom-style="--fa-popup-body-padding: 0;"
		body-scroll="none"
		:title="props.title"
		:height="props.height"
		:z-index="props.zIndex"
		position="bottom"
		closable
		:show-confirm-button="props.multiple"
		@confirm="handleConfirm"
		@open="visible = true"
		@close="visible = false"
		@after-enter="faPagingRef.reload()"
	>
		<template v-if="$slots.titlePrefix" #titlePrefix="{ loading }">
			<slot name="titlePrefix" :loading="loading" />
		</template>

		<FaPaging
			ref="faPagingRef"
			v-model:search-param="searchParam"
			:data="localData"
			:request-api="props.requestApi"
			:init-param="props.initParam"
			:pagination="props.pagination"
			:auto="false"
			:fixed="false"
			:refresher-enabled="!!props.requestApi"
			:show-refresher-when-reload="!!props.requestApi"
			:use-page-scroll="false"
			:use-virtual-list="props.useVirtualList"
		>
			<template v-if="props.initParam === undefined || typeof props.initParam === 'object'" #top="{ refresh }">
				<FaSearchInput
					v-model:search-param="searchParam"
					:filter="false"
					:disabled="props.disabledSearch"
					@confirm="props.requestApi && refresh()"
					@clear="props.requestApi && refresh()"
					@search="props.requestApi && refresh()"
				>
					<template #suffix>
						<wd-button v-if="props.clearable" size="mini" type="primary" variant="plain" @click="handleClear">清空</wd-button>
					</template>
				</FaSearchInput>
			</template>

			<template #default="{ pagedList }">
				<template v-if="props.treeSelect">
					<wd-cell-group v-for="item in handleData(pagedList)" :key="item.value" :title="item.label" border>
						<wd-cell
							v-for="cItem in item.children"
							:key="cItem.value"
							center
							:clickable="!cItem.disabled"
							:custom-class="isSelected(cItem.rawValue) ? 'fa-select-page__cell--selected' : ''"
							:custom-style="cItem.disabled ? 'opacity: 0.5;' : ''"
							@click="handleChange(cItem as TResult, item)"
						>
							<template #title>
								<slot
									:label="cItem.label"
									:value="cItem.value"
									:data="cItem.data"
									:hide="cItem.hide"
									:disabled="cItem.disabled"
									:children="cItem.children"
									:item="cItem"
								>
									{{ cItem.label }}
								</slot>
							</template>
							<template #suffix>
								<wd-icon v-if="isSelected(cItem.rawValue)" name="check" size="32rpx" color="var(--wot-primary-6)" />
							</template>
						</wd-cell>
					</wd-cell-group>
				</template>
				<wd-cell-group v-else border>
					<wd-cell
						v-for="item in handleData(pagedList)"
						:key="item.value"
						center
						:clickable="!item.disabled"
						:custom-class="isSelected(item.rawValue) ? 'fa-select-page__cell--selected' : ''"
						:custom-style="item.disabled ? 'opacity: 0.5;' : ''"
						@click="handleChange(item)"
					>
						<template #title>
							<slot
								:label="item.label"
								:value="item.value"
								:data="item.data"
								:hide="item.hide"
								:disabled="item.disabled"
								:children="item.children"
								:item="item"
							>
								{{ item.label }}
							</slot>
						</template>
						<template #suffix>
							<wd-icon v-if="isSelected(item.rawValue)" name="check" size="32rpx" color="var(--wot-primary-6)" />
						</template>
					</wd-cell>
				</wd-cell-group>
			</template>
		</FaPaging>

		<template v-if="$slots.footer || props.multiple" #footer="{ loading, close: _close }">
			<text v-if="props.multiple" class="fa-select-page__selected-count">已选 {{ selectedList.size }} 项</text>
			<slot v-if="$slots.footer" name="footer" :loading="loading" :close="_close" />
		</template>
	</FaPopup>
</template>

<script
	setup
	lang="ts"
	generic="
		TValue extends string | number | boolean,
		TResult extends ElSelectorOutput = ElSelectorOutput<TValue>,
		TInput extends string | number | boolean | PagedInput = PagedInput
	"
>
import { computed, ref, shallowRef, watch } from "vue";
import FaPaging from "../paging/index.vue";
import FaPopup from "../popup/index.vue";
import type { PropType, Ref } from "vue";
import type { ComponentExposed } from "vue-component-type-helpers";

defineOptions({
	name: "SelectPage",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 弹窗标题 */
	title: {
		type: String,
		default: "请选择",
	},
	/** 弹窗高度，数字按 px 处理 */
	height: {
		type: [String, Number],
		default: "80%",
	},
	/** 弹窗层级 */
	zIndex: {
		type: Number,
		default: 210,
	},
	/** 是否多选；确认时统一回写，搜索和翻页保留选择，点击清空时立即回写空值 */
	multiple: Boolean,
	/** 是否显示清空按钮，同时清空选中值、标签和搜索条件 */
	clearable: {
		type: Boolean,
		default: true,
	},
	/** 选项原始值所在的字段名 */
	valueKey: {
		type: String,
		default: "value",
	},
	/** 选项标签、显隐、禁用状态和子节点的字段映射 */
	props: {
		type: Object as PropType<{
			/** 标签字段名或根据选项计算标签的函数 */
			label?: string | ((data: TResult) => string);
			/** 隐藏状态字段名或判断函数，只有 true 表示隐藏 */
			hide?: string | ((data: TResult) => boolean);
			/** 禁用状态字段名或判断函数，只有 true 表示禁用 */
			disabled?: string | ((data: TResult) => boolean);
			/** 子节点数组所在的字段名 */
			children?: string;
		}>,
		default: () => ({
			label: "label",
			hide: "hide",
			disabled: "disabled",
			children: "children",
		}),
	},
	/** 是否禁用搜索输入框 */
	disabledSearch: Boolean,
	/** 完整本地选项列表，未提供 requestApi 时使用，支持标签搜索 */
	data: {
		type: Array as PropType<TResult[]>,
		default: undefined,
	},
	/** 远程查询函数，优先于本地 data；关闭分页时应返回完整数组 */
	requestApi: {
		type: Function as PropType<(params?: TInput) => Promise<PagedResult<TResult> | TResult[]>>,
	},
	/** 接口初始参数；标量直接传入，传对象时搜索参数覆盖同名字段 */
	initParam: {
		type: [String, Number, Boolean, Object] as PropType<TInput>,
		default: undefined,
	},
	/** 是否启用分页；关闭后一次展示完整列表 */
	pagination: {
		type: Boolean,
		default: true,
	},
	/** 是否按首层节点分组，仅渲染首层 children 作为可选项 */
	treeSelect: Boolean,
	/** 是否使用虚拟列表 */
	useVirtualList: Boolean,
	/** 弹窗初始化时执行的回调，可返回 Promise；多选草稿会在回调前按当前值与标签初始化 */
	openCallback: Function as PropType<() => void | Promise<void>>,
});

const emit = defineEmits<{
	/**
	 * 值或选择集合变化时触发；单选返回选项和父节点，多选返回选项对象数组。
	 * 多选优先使用当前页记录，未命中当前页的已选项仅包含原始值和标签；清空按钮固定返回 null、null。
	 */
	change: [value: TResult | TResult[] | null, parentValue?: TResult | null];
	/** 点击清空按钮时立即触发，同时重置搜索条件和所有选中项 */
	clear: [];
}>();

/** 弹窗显隐状态 */
const visible = defineModel<boolean>("visible", { default: false });
/** 单选为原始值，多选为原始值数组；清空时分别为 null 和空数组 */
const modelValue = defineModel<TValue | TValue[]>();
/** 单选为标签，多选为对应的标签数组；清空时分别为 null 和空数组 */
const selectedLabel = defineModel<string | string[]>("label");
// 多选草稿按原始值保存标签，不随搜索和翻页丢失；确认时再回写模型。
const selectedList = shallowRef(new Map<TValue, string>());

/** 分页搜索参数 */
const searchParam: Ref<Partial<TInput extends PagedInput ? TInput : PagedInput>> = ref({});

const faPopupRef = ref<InstanceType<typeof FaPopup>>();
const faPagingRef = ref<ComponentExposed<typeof FaPaging<TResult, TInput>>>();

const isSelected = (value?: TValue) => {
	if (props.multiple) {
		return selectedList.value.has(value);
	} else {
		return value === modelValue.value;
	}
};

const getLabel = (item: TResult) => {
	const { label } = props.props;
	const value: unknown = typeof label === "function" ? label(item) : item[label ?? "label"];
	return typeof value === "string" ? value : undefined;
};

const handleData = (data: TResult[]): TResult[] => {
	return (data ?? [])
		.map((item) => {
			const value: TValue | undefined = item[props.valueKey];
			const hide: unknown = typeof props.props.hide === "function" ? props.props.hide(item) : item[props.props.hide ?? "hide"];
			const disabled: unknown =
				typeof props.props.disabled === "function" ? props.props.disabled(item) : item[props.props.disabled ?? "disabled"];
			const children: unknown = item[props.props.children ?? "children"];
			const rawValue = typeof value === "string" || typeof value === "number" || typeof value === "boolean" ? value : undefined;
			return {
				...item,
				children: Array.isArray(children) ? handleData(children as TResult[]) : [],
				disabled: disabled === true,
				hide: hide === true,
				label: getLabel(item),
				value: `${typeof rawValue}:${rawValue}`,
				rawValue,
			};
		})
		.filter((item) => !item.hide);
};

const localData = computed(() => {
	const keyword = searchParam.value.searchValue?.trim().toLowerCase();
	if (!keyword || !props.data) return props.data;

	const childrenKey = props.props.children ?? "children";

	const filterItems = (items: TResult[]): TResult[] =>
		items.flatMap((item) => {
			// 当前节点匹配时，保留它的全部子节点。
			if (getLabel(item)?.toLowerCase().includes(keyword)) return [item];

			const children: unknown = item[childrenKey];
			if (!Array.isArray(children)) return [];

			// 子节点匹配时，保留父节点和命中的分支。
			const matchedChildren = filterItems(children as TResult[]);
			return matchedChildren.length ? [{ ...item, [childrenKey]: matchedChildren }] : [];
		});

	return filterItems(props.data);
});

const handleOpen = () => {
	if (faPopupRef.value && !faPopupRef.value.visible) {
		faPopupRef.value.open(() => {
			if (props.multiple) {
				const currentValues = Array.isArray(modelValue.value) ? modelValue.value : [];
				const currentLabels = Array.isArray(selectedLabel.value) ? selectedLabel.value : [];
				selectedList.value = new Map(currentValues.map((value, index) => [value, currentLabels[index] ?? ""]));
			}
			return props.openCallback?.();
		});
	}
};

const handleClose = () => {
	if (faPopupRef.value?.visible) {
		faPopupRef.value.close();
	}
};

const handleChange = (item?: TResult, parentItem?: TResult) => {
	const value = item?.rawValue;
	const label = item?.label ?? "";
	if (props.multiple) {
		const items = new Map(selectedList.value);
		if (!items.delete(value)) {
			items.set(value, label);
		}
		selectedList.value = items;
	} else {
		const hasChanged = (modelValue.value ?? undefined) !== value;

		modelValue.value = value;
		selectedLabel.value = label;

		if (hasChanged) {
			emit("change", item ? { ...item, value: item.rawValue } : null, parentItem ? { ...parentItem, value: parentItem.rawValue } : null);
		}

		handleClose();
	}
};

const handleConfirm = () => {
	if (!props.multiple) return;

	const values = [...selectedList.value.keys()];
	const currentValues = Array.isArray(modelValue.value) ? modelValue.value : [];
	const hasChanged = currentValues.length !== values.length || currentValues.some((value) => !selectedList.value.has(value));

	modelValue.value = values;
	selectedLabel.value = [...selectedList.value.values()];

	if (hasChanged) {
		const pagedListMap = new Map(faPagingRef.value.pagedList.map((item) => [item[props.valueKey], item]));
		const result = [...selectedList.value].map(
			([value, label]) =>
				pagedListMap.get(value) ?? {
					[props.valueKey]: value,
					label,
				}
		);

		emit("change", result as TResult[], null);
	}

	handleClose();
};

const handleClear = () => {
	faPagingRef.value?.clearSearchParam();
	if (props.multiple) {
		modelValue.value = [];
		selectedLabel.value = [];
		selectedList.value = new Map();
	} else {
		modelValue.value = null;
		selectedLabel.value = null;
	}
	emit("clear");
	emit("change", null, null);
};

watch([visible, faPopupRef], ([isVisible]) => (isVisible ? handleOpen() : handleClose()), { immediate: true, flush: "post" });

defineExpose({
	/** 已加载的数据 */
	pagedList: computed(() => faPagingRef.value?.pagedList),
	/** 当前搜索参数，不会因列表翻页而自动清空 */
	searchParam: computed(() => searchParam.value),
	/** 请求打开选择弹窗并执行初始化流程；此方法不返回初始化完成的 Promise。 */
	open: handleOpen,
	/** 请求关闭选择弹窗。 */
	close: handleClose,
	/** 执行异步操作并在结束时恢复加载状态；异常继续向调用方传播。 */
	doLoading: computed(() => faPopupRef.value?.doLoading),
});
</script>
