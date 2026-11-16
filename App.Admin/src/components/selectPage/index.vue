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

		<template v-if="props.multiple" #footer>
			<text class="fa-select-page__selected-count">已选 {{ selectedList.size }} 项</text>
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
	/** 弹窗高度 */
	height: {
		type: [String, Number],
		default: "80%",
	},
	/** 弹窗层级 */
	zIndex: {
		type: Number,
		default: 210,
	},
	/** 是否多选。多选时在确认后提交全部选中的原始值 */
	multiple: Boolean,
	/** 是否允许清空当前选中值 */
	clearable: {
		type: Boolean,
		default: true,
	},
	/** 值Key */
	valueKey: {
		type: String,
		default: "value",
	},
	/** 配置选项 */
	props: {
		type: Object as PropType<{
			/** 指定标签为节点的某个属性值 */
			label?: string | ((data: TResult) => string);
			/** 指定是否隐藏为节点的某个属性值 */
			hide?: string | ((data: TResult) => boolean);
			/** 指定是否禁用为节点的某个属性值 */
			disabled?: string | ((data: TResult) => boolean);
			/** 指定子节点对象为节点的某个属性值 */
			children?: string;
		}>,
		default: () => ({
			label: "label",
			hide: "hide",
			disabled: "disabled",
			children: "children",
		}),
	},
	/** 禁用搜素输入框 */
	disabledSearch: Boolean,
	/** 完整的本地列表，空数组表示空数据 */
	data: {
		type: Array as PropType<TResult[]>,
		default: undefined,
	},
	/** 请求数据的函数 */
	requestApi: {
		type: Function as PropType<(params?: TInput) => Promise<PagedResult<TResult> | TResult[]>>,
	},
	/** 初始化参数 */
	initParam: {
		type: [String, Number, Boolean, Object] as PropType<TInput>,
		default: undefined,
	},
	/** 分页 */
	pagination: {
		type: Boolean,
		default: true,
	},
	/** 树形选择，只支持一级节点 */
	treeSelect: Boolean,
	/** 是否使用虚拟列表 */
	useVirtualList: Boolean,
	/** 打开回调 */
	openCallback: Function as PropType<() => void | Promise<void>>,
});

const emit = defineEmits<{
	/** 单选返回选项及其父节点，清空时返回 null。多选确认时返回原始值数组 */
	change: [value: TResult | TResult[] | null, parentValue?: TResult | null];
	/** 单选立即触发。多选在确认将已有选中值清空时触发 */
	clear: [];
}>();

/** 弹窗显隐状态 */
const visible = defineModel<boolean>("visible", { default: false });
/** 当前选中的原始值。多选模式下为原始值数组 */
const modelValue = defineModel<TValue | TValue[]>();
/** 单选模式下当前选中项的标签 */
const selectedLabel = defineModel<string | string[]>("label");
/** 选中数据 */
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

const open = () => {
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

const close = () => {
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
		modelValue.value = value;
		selectedLabel.value = label;
		emit("change", item ? { ...item, value: item.rawValue } : null, parentItem ? { ...parentItem, value: parentItem.rawValue } : null);
		close();
	}
};

const handleConfirm = () => {
	if (!props.multiple) return;
	modelValue.value = [...selectedList.value.keys()];
	selectedLabel.value = [...selectedList.value.values()];
	const pagedListMap = new Map(faPagingRef.value.pagedList.map((item) => [item[props.valueKey], item]));
	const result = [...selectedList.value].map(
		([value, label]) =>
			pagedListMap.get(value) ?? {
				[props.valueKey]: value,
				label,
			}
	);
	emit("change", result as TResult[], null);
	close();
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

watch([visible, faPopupRef], ([isVisible]) => (isVisible ? open() : close()), { immediate: true, flush: "post" });

defineExpose({
	/** 已加载的数据 */
	pagedList: computed(() => faPagingRef.value?.pagedList),
	/** 当前搜索参数 */
	searchParam: computed(() => searchParam.value),
	open,
	close,
	/** 执行异步操作并在结束时恢复加载状态；异常继续向调用方传播。 */
	doLoading: computed(() => faPopupRef.value?.doLoading),
});
</script>
