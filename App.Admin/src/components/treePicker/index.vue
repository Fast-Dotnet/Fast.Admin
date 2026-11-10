<template>
	<FaPopup
		ref="faPopupRef"
		custom-class="fa-tree-picker"
		:title="props.title"
		:height="props.height"
		:z-index="props.zIndex"
		position="bottom"
		closable
		:show-confirm-button="props.checkStrictly"
		@open="visible = true"
		@after-enter="updateTabLine"
		@close="visible = false"
		@confirm="handleConfirm"
	>
		<template #default="{ loading }">
			<FaSearchInput
				v-model:search-param="searchParam"
				:filter="false"
				:disabled="props.disabledSearch"
				@confirm="handleSearch"
				@clear="handleSearch"
				@search="handleSearch"
			>
				<template #suffix>
					<wd-button v-if="props.clearable" size="mini" type="primary" variant="plain" :disabled="loading" @click="handleClear">
						清空
					</wd-button>
				</template>
			</FaSearchInput>

			<wd-empty v-if="hasError" :icon-size="140" icon="no-result" tip="加载失败，请稍后重试~">
				<template #bottom>
					<wd-button type="info" variant="plain" @click="handleSearch">立即重试</wd-button>
				</template>
			</wd-empty>

			<template v-else-if="tabList.length">
				<wd-tabs
					ref="tabsRef"
					custom-class="fa-tree-picker__tabs"
					v-model="activeTab"
					slidable="always"
					line-theme="underline"
					:slidable-num="3"
					:map-num="0"
				>
					<wd-tab v-for="(tab, index) in tabList" :key="index" :name="index" :title="tab.title" />
				</wd-tabs>
				<scroll-view :key="activeTab" class="fa-tree-picker__list" scroll-y>
					<wd-cell-group border>
						<wd-cell
							v-for="item in tabList[activeTab]?.options"
							:key="item.value"
							:clickable="!item.disabled"
							:custom-class="selectedList[activeTab] === item ? 'fa-tree-picker__cell--selected' : ''"
							:custom-style="item.disabled ? 'opacity: 0.5;' : ''"
							@click="handleSelect(item, activeTab)"
						>
							<template #title>
								<slot v-bind="item">
									{{ item.label }}
								</slot>
							</template>
							<template #suffix>
								<wd-icon v-if="selectedList[activeTab] === item" name="check" size="32rpx" color="var(--wot-primary-6)" />
							</template>
						</wd-cell>
					</wd-cell-group>
				</scroll-view>
			</template>

			<wd-empty v-else-if="!loading" icon="no-content" tip="暂无数据~" />
		</template>
	</FaPopup>
</template>

<script
	setup
	lang="ts"
	generic="
		TValue extends string | number | boolean,
		TResult extends ElTreeOutput = ElTreeOutput<TValue>,
		TInput extends string | number | boolean | PagedInput = PagedInput
	"
>
import { computed, nextTick, ref, shallowRef, watch } from "vue";
import type { TabsInstance } from "@wot-ui/ui/components/wd-tabs/types";
import type { PropType } from "vue";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "TreePicker",
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
		default: 10,
	},
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
	/** 允许确认任意层级的节点 */
	checkStrictly: Boolean,
	/** 禁用搜素输入框 */
	disabledSearch: Boolean,
	/** 完整的本地列表，空数组表示空数据 */
	data: {
		type: Array as PropType<TResult[]>,
		default: undefined,
	},
	/** 请求数据的函数 */
	requestApi: {
		type: Function as PropType<(params?: TInput) => Promise<TResult[]>>,
	},
	/** 初始化参数 */
	initParam: {
		type: [String, Number, Boolean, Object] as PropType<TInput>,
		default: undefined,
	},
	/** 打开弹窗并加载树形数据后执行的回调 */
	openCallback: Function as PropType<() => void | Promise<void>>,
});

const emit = defineEmits<{
	/** 返回提交的节点和完整路径，value 为原始值，确认空路径或清空时两个参数均为 null */
	change: [value: TResult | null, values: TResult[] | null];
}>();

/** 弹窗显隐状态 */
const visible = defineModel<boolean>("visible", { default: false });
/** 选中节点的原始值 */
const modelValue = defineModel<TValue>();
/** 当前选中项的标签 */
const selectedLabel = defineModel<string>("label");

/** 搜索参数 */
const searchParam = ref<Partial<PagedInput>>({});

const faPopupRef = ref<FaPopupInstance>();
const tabsRef = ref<TabsInstance>();

/** 激活的 Tab */
const activeTab = shallowRef<number>(0);
/** 存在错误 */
const hasError = shallowRef(false);
/** 树形数据 */
const treeList = shallowRef<TResult[]>([]);
/** 实际选中的节点路径 */
const selectedList = shallowRef<TResult[]>([]);

const tabList = computed(() => {
	const levels: { title: string; options: TResult[] }[] = [];
	let options = treeList.value;
	for (const item of selectedList.value) {
		levels.push({ title: item.label ?? "请选择", options });
		options = item.children as TResult[];
	}
	if (options.length) levels.push({ title: "请选择", options });
	return levels;
});

const updateTabLine = () => {
	if (faPopupRef.value?.visible) tabsRef.value?.updateLineStyle(false);
};

const getLabel = (item: TResult) => {
	const { label } = props.props;
	const value: unknown = typeof label === "function" ? label(item) : item[label ?? "label"];
	return typeof value === "string" ? value : undefined;
};

const handleData = (data: TResult[]): TResult[] => {
	return data
		.map((item) => {
			const value: unknown = item[props.valueKey];
			const hide: unknown = typeof props.props.hide === "function" ? props.props.hide(item) : item[props.props.hide ?? "hide"];
			const disabled: unknown =
				typeof props.props.disabled === "function" ? props.props.disabled(item) : item[props.props.disabled ?? "disabled"];
			const children: unknown = item[props.props.children ?? "children"];
			const selectorValue = typeof value === "string" || typeof value === "number" || typeof value === "boolean" ? value : undefined;
			return {
				...item,
				children: Array.isArray(children) ? handleData(children as TResult[]) : [],
				disabled: disabled === true,
				hide: hide === true,
				label: getLabel(item),
				value: `${typeof selectorValue}:${selectorValue}`,
				rawValue: selectorValue,
			};
		})
		.filter((item) => !item.hide);
};

const findPath = (items: TResult[], value: TValue): TResult[] | undefined => {
	for (const item of items) {
		if (item.rawValue === value) return [item];
		const children = findPath(item.children as TResult[], value);
		if (children) return [item, ...children];
	}
	return undefined;
};

const setPath = (path: TResult[]) => {
	selectedList.value = path;
	// 等新层级的 wd-tab 注册后再切换。
	nextTick(() => {
		if (selectedList.value === path) {
			activeTab.value = Math.max(0, tabList.value.length - 1);
		}
	});
};

const restorePath = () => {
	const value = modelValue.value;
	setPath(value == null ? [] : (findPath(treeList.value, value) ?? []));
};

const loadData = async () => {
	try {
		hasError.value = false;
		if (props.requestApi) {
			const params =
				typeof props.initParam === "string" || typeof props.initParam === "number" || typeof props.initParam === "boolean"
					? props.initParam
					: ({
							...(typeof props.initParam === "object" ? props.initParam : {}),
							...searchParam.value,
						} as TInput);
			const resData = await props.requestApi(params);
			treeList.value = handleData(resData);
		} else {
			const keyword = searchParam.value.searchValue?.trim().toLowerCase();
			const items = handleData(props.data ?? []);
			const filterItems = (nodes: TResult[]): TResult[] =>
				nodes.flatMap((item) => {
					// 命中当前节点时保留整条分支，否则只保留命中的后代和它们的祖先。
					if (item.label?.toLowerCase().includes(keyword)) return [item];
					const children = filterItems(item.children as TResult[]);
					return children.length ? [{ ...item, children }] : [];
				});
			treeList.value = keyword ? filterItems(items) : items;
		}
		restorePath();
	} catch (error) {
		hasError.value = true;
		console.error(error);
	}
};

const open = () => {
	if (faPopupRef.value && !faPopupRef.value.visible) {
		faPopupRef.value.open(async () => {
			await loadData();
			await props.openCallback?.();
		});
	}
};

const close = () => {
	if (faPopupRef.value?.visible) {
		faPopupRef.value.close();
	}
};

const handleSearch = () => {
	if (!faPopupRef.value?.visible || faPopupRef.value.loading) return;
	faPopupRef.value.doLoading(loadData);
};

const handleConfirm = () => {
	const item = selectedList.value.at(-1);
	const path =
		item?.rawValue === undefined
			? []
			: selectedList.value.flatMap((node) => (node.rawValue === undefined ? [] : [{ ...node, value: node.rawValue }]));
	const selected = path.at(-1);
	modelValue.value = selected?.value;
	selectedLabel.value = selected?.label;
	emit("change", selected ?? null, selected ? path : null);
	close();
};

const handleSelect = (item: TResult, level: number) => {
	if (item.disabled || faPopupRef.value?.loading) return;
	if (selectedList.value[level] === item && item.children.length) {
		activeTab.value = level + 1;
		return;
	}
	const path = [...selectedList.value.slice(0, level), item];
	setPath(path);
	if (selectedList.value === path && visible.value && !props.checkStrictly && !item.children.length) {
		handleConfirm();
	}
};

const handleClear = () => {
	modelValue.value = undefined;
	selectedLabel.value = undefined;
	searchParam.value = {};
	emit("change", null, null);
	setPath([]);
	close();
};

// 标题宽度变化时，当前 Tab 可能不变，需要重新测量指示线。
watch(tabList, () => nextTick(updateTabLine), { flush: "post" });

watch(modelValue, () => {
	if (visible.value && faPopupRef.value?.visible && !faPopupRef.value.loading) {
		restorePath();
		return;
	}
	return undefined;
});

watch([visible, faPopupRef], ([isVisible]) => (isVisible ? open() : close()), { immediate: true, flush: "post" });

defineExpose({
	/** 已加载的数据 */
	treeList: computed(() => treeList.value),
	open,
	close,
	/** 执行异步操作并在结束时恢复加载状态；异常继续向调用方传播。 */
	doLoading: computed(() => faPopupRef.value?.doLoading),
});
</script>

<style lang="scss">
.fa-tree-picker {
	&.fa-popup .fa-popup__body {
		padding: 0;
		overflow: hidden;
	}

	.fa-search-input {
		flex-shrink: 0;
		border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
	}

	.fa-tree-picker__tabs {
		flex-shrink: 0;
	}

	.fa-tree-picker__list {
		flex: 1;
		min-height: 0;
		height: 0;
		background: var(--wot-filled-oppo);

		.wd-cell__wrapper {
			align-items: center;
		}

		.wd-cell__left {
			flex: 1;
		}

		.wd-cell__right {
			flex: unset;
		}
	}

	.fa-tree-picker__cell--selected {
		--wot-cell-bg: var(--wot-primary-1);
	}
}

.wot-theme-dark .fa-tree-picker .fa-tree-picker__cell--selected {
	--wot-cell-bg: var(--wot-primary-3);
}
</style>
