<template>
	<FaPopup
		ref="popupRef"
		:title="props.title"
		:height="props.height"
		:z-index="props.zIndex"
		position="bottom"
		closable
		@open="visible = true"
		@close="visible = false"
		@after-enter="faPagingRef?.reload()"
	>
		<FaPaging
			ref="faPagingRef"
			v-model="pagedList"
			v-model:search-param="searchParam"
			:data="localData"
			:request-api="props.requestApi"
			:pagination="props.pagination"
			:auto="false"
			:use-page-scroll="false"
		>
			<template #top="{ refresh }">
				<FaSearchInput
					v-model:search-param="searchParam"
					:filter="false"
					@confirm="props.requestApi && refresh()"
					@clear="props.requestApi && refresh()"
					@search="props.requestApi && refresh()"
				>
					<template #suffix>
						<wd-button v-if="props.clearable" size="mini" type="primary" variant="plain" @click="handleClear">清空</wd-button>
					</template>
				</FaSearchInput>
			</template>

			<template #default>
				<slot :paged-list="localPagedList" :select="handleChange">
					<wd-radio-group :model-value="radioValue">
						<wd-cell-group border>
							<wd-cell
								v-for="item in localPagedList"
								:key="item.value"
								:title="item.label"
								:clickable="!item.disabled"
								@click="handleChange(item)"
							>
								<template #suffix>
									<wd-radio :value="item.value" :disabled="item.disabled" />
								</template>
							</wd-cell>
						</wd-cell-group>
					</wd-radio-group>
				</slot>
			</template>
		</FaPaging>
	</FaPopup>
</template>

<script
	setup
	lang="ts"
	generic="
		TValue extends string | number | boolean,
		TResult extends ElSelectorOutput = ElSelectorOutput<TValue>,
		TInput extends PagedInput = PagedInput
	"
>
import { type PropType, type ShallowRef, computed, ref, shallowRef, watch } from "vue";
import FaPaging from "../paging/index.vue";
import FaPopup from "../popup/index.vue";
import FaSearchInput from "../searchInput/index.vue";
import type { FaPagingInstance, FaPopupInstance } from "@/components";

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
		default: 15,
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
		}>,
		default: () => ({
			label: "label",
			hide: "hide",
			disabled: "disabled",
		}),
	},
	/** 完整的本地列表，空数组表示空数据 */
	data: {
		type: Array as PropType<TResult[]>,
		default: undefined,
	},
	/** 请求数据的函数 */
	requestApi: {
		type: Function as PropType<(params?: Partial<TInput>) => Promise<PagedResult<TResult> | TResult[]>>,
	},
	/** 分页 */
	pagination: {
		type: Boolean,
		default: true,
	},
});

const emit = defineEmits<{
	/** 选择时返回映射后的选项，value 为原始值，清空选择时返回 null */
	change: [value: TResult | null];
}>();

/** 弹窗显隐状态 */
const visible = defineModel<boolean>("visible", { default: false });
/** 当前选中的值 */
const modelValue = defineModel<TValue>();
/** 当前选中项的标签 */
const selectedLabel = defineModel<string>("label");
const radioValue = computed(() => (modelValue.value == null ? null : `${typeof modelValue.value}:${modelValue.value}`));

const popupRef = ref<FaPopupInstance>();
const faPagingRef = ref<FaPagingInstance>();

const pagedList = shallowRef<TResult[]>([]);
const searchParam: ShallowRef<Partial<TInput>> = shallowRef({});

const getLabel = (item: TResult) => {
	const { label } = props.props;
	const value: unknown = typeof label === "function" ? label(item) : item[label ?? "label"];
	return typeof value === "string" ? value : undefined;
};

const handleData = (data: TResult[]) => {
	return data
		?.flatMap((item) => {
			const value: unknown = item[props.valueKey];
			const hide: unknown = typeof props.props.hide === "function" ? props.props.hide(item) : item[props.props.hide ?? "hide"];
			const disabled: unknown =
				typeof props.props.disabled === "function" ? props.props.disabled(item) : item[props.props.disabled ?? "disabled"];
			const selectorValue =
				value !== null && (typeof value === "string" || typeof value === "number" || typeof value === "boolean") ? value : undefined;
			return {
				...item,
				disabled: typeof disabled === "boolean" ? disabled : undefined,
				hide: typeof hide === "boolean" ? hide : undefined,
				label: getLabel(item),
				value: `${typeof selectorValue}:${selectorValue}`,
				rawValue: selectorValue,
			};
		})
		.filter((item) => item.hide !== true);
};

const localData = computed(() => {
	const keyword = searchParam.value.searchValue?.trim().toLowerCase();
	if (!keyword) return props.data;
	return props.data?.filter((item) => getLabel(item)?.toLowerCase().includes(keyword));
});

const localPagedList = computed(() => handleData(pagedList.value));

const open = () => {
	if (popupRef.value && !popupRef.value.visible) {
		popupRef.value.open();
	}
};

const close = () => {
	if (popupRef.value?.visible) {
		popupRef.value.close();
	}
};

const handleChange = (item?: TResult) => {
	modelValue.value = item?.rawValue;
	selectedLabel.value = item?.label;
	emit("change", item ? { ...item, value: item?.rawValue } : null);
	if (item?.rawValue != null) close();
};

const handleClear = () => {
	searchParam.value = { ...searchParam.value, searchValue: null };
	handleChange();
	faPagingRef.value.reload();
};

watch([visible, popupRef], ([isVisible]) => (isVisible ? open() : close()), { immediate: true, flush: "post" });

defineExpose({
	open,
	close,
});
</script>

<style lang="scss" scoped>
.fa-search-input {
	border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
}

:deep(.fa-popup__body) {
	padding: 0;
}

:deep(.zp-paging-container) {
	padding: var(--fa-spacing-page) 0 0 0;
}
</style>
