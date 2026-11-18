<template>
	<FaPopup
		ref="faPopupRef"
		custom-class="fa-tree-picker"
		custom-style="--fa-popup-body-padding: 0;"
		body-scroll="none"
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
		<template v-if="$slots.titlePrefix" #titlePrefix="{ loading }">
			<slot name="titlePrefix" :loading="loading" />
		</template>

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
				<wd-tabs ref="tabsRef" v-model="activeTab" slidable="always" line-theme="underline" :slidable-num="3" :map-num="0">
					<wd-tab v-for="(tab, index) in tabList" :key="index" :name="index" :title="tab.title" />
				</wd-tabs>
				<scroll-view :key="activeTab" class="fa-tree-picker__list" scroll-y>
					<wd-cell-group border>
						<wd-cell
							v-for="item in tabList[activeTab]?.options"
							:key="item.value"
							center
							:clickable="!item.disabled"
							:custom-class="selectedList[activeTab] === item ? 'fa-tree-picker__cell--selected' : ''"
							:custom-style="item.disabled ? 'opacity: 0.5;' : ''"
							@click="handleSelect(item, activeTab)"
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
								<wd-icon v-if="selectedList[activeTab] === item" name="check" size="32rpx" color="var(--wot-primary-6)" />
							</template>
						</wd-cell>
					</wd-cell-group>
				</scroll-view>
			</template>

			<wd-empty v-else-if="!loading" icon="no-content" tip="暂无数据~" />
		</template>

		<template v-if="$slots.footer" #footer="{ loading, close }">
			<slot name="footer" :loading="loading" :close="close" />
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
import FaPopup from "../popup/index.vue";
import type { TabsInstance } from "@wot-ui/ui/components/wd-tabs/types";
import type { PropType, Ref } from "vue";

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
	/** 是否允许清空当前选中值 */
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
	/** 是否允许确认任意层级；关闭时点击叶子节点立即提交 */
	checkStrictly: Boolean,
	/** 是否禁用搜索输入框 */
	disabledSearch: Boolean,
	/** 完整本地树，未提供 requestApi 时使用；搜索保留命中节点及其祖先路径 */
	data: {
		type: Array as PropType<TResult[]>,
		default: undefined,
	},
	/** 返回完整树形数组的查询函数，优先于本地 data */
	requestApi: {
		type: Function as PropType<(params?: TInput) => Promise<TResult[]>>,
	},
	/** 接口初始参数；标量直接传入，传对象时搜索参数覆盖同名字段 */
	initParam: {
		type: [String, Number, Boolean, Object] as PropType<TInput>,
		default: undefined,
	},
	/** 树形数据加载和已选路径恢复后执行的初始化回调 */
	openCallback: Function as PropType<() => void | Promise<void>>,
});

const emit = defineEmits<{
	/** 返回提交的节点和完整路径，value 为原始值，确认空路径或清空时两个参数均为 null */
	change: [value: TResult | null, values: TResult[] | null];
	/** 点击清空按钮时触发，同时清除选中路径、标签和搜索条件并关闭弹窗 */
	clear: [];
}>();

/** 弹窗显隐状态 */
const visible = defineModel<boolean>("visible", { default: false });
/** 选中节点的原始值 */
const modelValue = defineModel<TValue>();
/** 当前选中项的标签 */
const selectedLabel = defineModel<string>("label");

/** 搜索参数 */
const searchParam: Ref<Partial<TInput extends PagedInput ? TInput : PagedInput>> = ref({});

const faPopupRef = ref<InstanceType<typeof FaPopup>>();
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
			const requestParam = {};
			const params =
				typeof props.initParam === "string" || typeof props.initParam === "number" || typeof props.initParam === "boolean"
					? props.initParam
					: Object.assign(requestParam, props.initParam, searchParam.value);
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

const handleOpen = () => {
	if (faPopupRef.value && !faPopupRef.value.visible) {
		faPopupRef.value.open(async () => {
			await loadData();
			await props.openCallback?.();
		});
	}
};

const handleClose = () => {
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
	const hasChanged = (modelValue.value ?? undefined) !== item?.rawValue;
	const path =
		item?.rawValue === undefined
			? []
			: selectedList.value.flatMap((node) => (node.rawValue === undefined ? [] : [{ ...node, value: node.rawValue }]));
	const selected = path.at(-1);

	modelValue.value = item?.rawValue;
	selectedLabel.value = selected?.label;

	if (hasChanged) {
		emit("change", selected ?? null, selected ? path : null);
	}

	handleClose();
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
	emit("clear");
	emit("change", null, null);
	setPath([]);
	handleClose();
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

watch([visible, faPopupRef], ([isVisible]) => (isVisible ? handleOpen() : handleClose()), { immediate: true, flush: "post" });

defineExpose({
	/** 经过字段映射和隐藏过滤后的树形数据 */
	treeList: computed(() => treeList.value),
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
