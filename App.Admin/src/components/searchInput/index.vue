<template>
	<view class="fa-search-input">
		<view class="fa-search-input__header">
			<slot name="prefix" />
			<wd-input
				v-model="modelValue"
				custom-class="fa-search-input__input"
				:disabled="props.disabled"
				clearable
				:placeholder="props.placeholder ? props.placeholder : props.scan ? '请输入或扫描您想要搜索的内容' : '请输入您想要搜索的内容'"
				@input="handleInput"
				@focus="(detail: UniHelper.InputOnFocusDetail) => emit('focus', detail)"
				@blur="(detail: UniHelper.InputOnBlurDetail) => emit('blur', detail)"
				@confirm="(detail: UniHelper.InputOnConfirmDetail) => emit('confirm', detail)"
				@clear="handleClear"
			>
				<template #prefix>
					<view style="display: flex">
						<FaIcon v-if="props.scan" custom-class="wd-input__icon" name="scan" @click="handleScanClick" />
						<wd-icon v-else custom-class="wd-input__icon" name="search" />
					</view>
				</template>
				<template #suffix v-if="props.search">
					<wd-button size="small" type="primary" @click="() => emit('search')">搜素</wd-button>
				</template>
			</wd-input>
			<view v-if="props.filter" class="fa-search-input__filter" @click="filterPopupRef?.open()">
				<FaIcon custom-class="wd-input__icon" name="filter" />
				<text class="fa-search-input__filter-text">筛选</text>
			</view>
			<slot name="suffix" />
		</view>
		<view v-if="$slots.footer" class="fa-search-input__footer">
			<slot name="footer" />
		</view>
		<FaPopup
			v-if="props.filter"
			ref="filterPopupRef"
			custom-class="fa-search-input__popup"
			title="高级筛选"
			height="60%"
			position="bottom"
			closable
			show-confirm-button
			confirm-button-text="筛选"
			@confirm="handleFilterClick"
		>
			<scroll-view class="fa-search-input__filters" scroll-y>
				<template v-if="!props.hideSearchTime">
					<FaCellPicker
						title="时间"
						:value="
							state.searchTimeList?.length > 0
								? `${dayjs(state.searchTimeList[0]).format('YYYY-MM-DD')} - ${dayjs(state.searchTimeList[1]).format('YYYY-MM-DD')}`
								: ''
						"
						placeholder="请选择日期"
					>
						<template #default="{ visible }">
							<wd-datetime-picker
								:disable="state.dataSearchRange !== 'custom'"
								type="date"
								v-model="state.searchTimeList"
								v-model:visible="visible.value"
								align-right
								@confirm="handleDateTimeConfirm"
							/>

							<wd-radio-group
								custom-class="fa-search-input__date-ranges"
								v-model="state.dataSearchRange"
								direction="horizontal"
								type="button"
								@change="handleDateRangeChange"
							>
								<wd-radio v-for="(item, index) in dataSearchRangeList" :key="index" :value="item.value">
									{{ item.label }}
								</wd-radio>
								<wd-radio value="custom">自定义</wd-radio>
							</wd-radio-group>
						</template>
					</FaCellPicker>
				</template>

				<slot name="filter" :search-param="slotState.searchParam" />
			</scroll-view>

			<template #footer="{ loading }">
				<wd-button custom-style="flex: 1;" type="info" variant="plain" block :disabled="loading" @click="handleFilterReset">重置</wd-button>
			</template>
		</FaPopup>
	</view>
</template>

<script setup lang="ts" generic="TInput extends PagedInput = PagedInput">
import { computed, onMounted, reactive, ref, watch } from "vue";
import { definePropType, throttle, withDefineType } from "@fast-china/utils";
import dayjs from "dayjs";
import FaPopup from "../popup/index.vue";

defineOptions({
	name: "SearchInput",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 占位文本 */
	placeholder: String,
	/** 显示扫描图标 */
	scan: {
		type: Boolean,
		default: false,
	},
	/** 显示搜索按钮 */
	search: {
		type: Boolean,
		default: false,
	},
	/** 显示筛选图标 */
	filter: {
		type: Boolean,
		default: true,
	},
	/** 禁用搜素输入框 */
	disabled: Boolean,
	/** 隐藏搜索时间 */
	hideSearchTime: Boolean,
	/** 是否使用从今天起向未来延伸的快捷日期范围 */
	futureSearchTime: Boolean,
	/** 搜索时间范围 */
	dataSearchRange: {
		type: definePropType<FaTableDataRange>(String),
		default: "Past3D",
	},
});

const emit = defineEmits<{
	/** 输入内容变化事件 */
	input: [detail: UniHelper.InputOnInputDetail];
	/** 输入框获得焦点事件 */
	focus: [detail: UniHelper.InputOnFocusDetail];
	/** 输入框失去焦点事件 */
	blur: [detail: UniHelper.InputOnBlurDetail];
	/** 键盘确认事件 */
	confirm: [detail: UniHelper.InputOnConfirmDetail];
	/** 清空输入事件 */
	clear: [];
	/** 搜索按钮或筛选确认按钮点击事件 */
	search: [];
}>();

/** 搜索文本，与 searchParam.searchValue 双向同步 */
const modelValue = defineModel<string>();
/** 分页搜索参数，包含搜索文本和格式化后的日期范围 */
const searchParam = defineModel<Partial<TInput>>("searchParam", { default: () => ({}) });
const slotState = { searchParam };

const filterPopupRef = ref<InstanceType<typeof FaPopup>>();

const state = reactive({
	searchTimeList: withDefineType<number[]>([]),
	dataSearchRange: withDefineType<FaTableDataRange | "custom">(),
	dataSearchPickerVisible: false,
});

const dataSearchRangeList = computed<ElSelectorOutput<FaTableDataRange>[]>(() => [
	{
		label: props.futureSearchTime ? "后1天" : "近1天",
		value: "Past1D",
	},
	{
		label: props.futureSearchTime ? "后3天" : "近3天",
		value: "Past3D",
	},
	{
		label: props.futureSearchTime ? "后1周" : "近1周",
		value: "Past1W",
	},
	{
		label: props.futureSearchTime ? "后1月" : "近1月",
		value: "Past1M",
	},
	{
		label: props.futureSearchTime ? "后3月" : "近3月",
		value: "Past3M",
	},
	{
		label: props.futureSearchTime ? "后6月" : "近6月",
		value: "Past6M",
	},
	{
		label: props.futureSearchTime ? "后1年" : "近1年",
		value: "Past1Y",
	},
]);

const getDateTime = (dataRange: FaTableDataRange | "custom") => {
	const end = new Date();
	const start = new Date();
	if (props.futureSearchTime) {
		switch (dataRange) {
			case "Past1D":
				end.setDate(end.getDate() + 1);
				break;
			case "Past3D":
				end.setDate(end.getDate() + 3);
				break;
			case "Past1W":
				end.setDate(end.getDate() + 7);
				break;
			case "Past1M":
				end.setMonth(end.getMonth() + 1);
				break;
			case "Past3M":
				end.setMonth(end.getMonth() + 3);
				break;
			case "Past6M":
				end.setMonth(end.getMonth() + 6);
				break;
			case "Past1Y":
				end.setFullYear(end.getFullYear() + 1);
				break;
			case "Past3Y":
				end.setFullYear(end.getFullYear() + 3);
				break;
		}
	} else {
		switch (dataRange) {
			case "Past1D":
				start.setDate(start.getDate() - 1);
				break;
			case "Past3D":
				start.setDate(start.getDate() - 3);
				break;
			case "Past1W":
				start.setDate(start.getDate() - 7);
				break;
			case "Past1M":
				start.setMonth(start.getMonth() - 1);
				break;
			case "Past3M":
				start.setMonth(start.getMonth() - 3);
				break;
			case "Past6M":
				start.setMonth(start.getMonth() - 6);
				break;
			case "Past1Y":
				start.setFullYear(start.getFullYear() - 1);
				break;
			case "Past3Y":
				start.setFullYear(start.getFullYear() - 3);
				break;
		}
	}
	return {
		start,
		end,
	};
};

/** 接口接收按天扩展的本地时间字符串，日期选择器使用毫秒时间戳。 */
const handleDateTimeConfirm = ({ value }: { value: number[] }) => {
	state.dataSearchRange = "custom";
	searchParam.value.searchTimeList = [dayjs(value[0]).format("YYYY-MM-DD 00:00:00"), dayjs(value[1]).format("YYYY-MM-DD 23:59:59")];
};

const handleDateRangeChange = ({ value }: { value: FaTableDataRange | "custom" }) => {
	state.dataSearchRange = value;
	const { start, end } = getDateTime(value);
	state.searchTimeList = [start.getTime(), end.getTime()];
	searchParam.value.searchTimeList = [dayjs(start).format("YYYY-MM-DD 00:00:00"), dayjs(end).format("YYYY-MM-DD 23:59:59")];
};

const defaultSearchTime = (params: Partial<TInput> = searchParam.value) => {
	if (!props.filter || props.hideSearchTime) {
		state.searchTimeList = [];
		searchParam.value = params;
	} else {
		const { start, end } = getDateTime(props.dataSearchRange);
		state.dataSearchRange = props.dataSearchRange;
		state.searchTimeList = [start.getTime(), end.getTime()];
		searchParam.value = {
			...params,
			searchTimeList: [dayjs(start).format("YYYY-MM-DD 00:00:00"), dayjs(end).format("YYYY-MM-DD 23:59:59")],
		};
	}
};

const handleScanClick = throttle(() => {
	uni.scanCode({
		onlyFromCamera: true,
		scanType: ["barCode", "qrCode", "datamatrix", "pdf417"],
		success: ({ result }) => {
			modelValue.value = result;
			searchParam.value.searchValue = result;
		},
	});
});

const handleInput = (detail: UniHelper.InputOnInputDetail) => {
	searchParam.value = { ...searchParam.value, searchValue: detail.value };
	emit("input", detail);
};

const handleClear = () => {
	searchParam.value = { ...searchParam.value, searchValue: "" };
	emit("clear");
};

const handleFilterClick = () => filterPopupRef.value?.close(() => emit("search"));

const handleFilterReset = () => {
	filterPopupRef.value?.close(() => {
		defaultSearchTime({});
		emit("search");
	});
};

watch(
	() => modelValue.value,
	(newVal) => {
		if (searchParam.value.searchValue !== newVal) {
			searchParam.value.searchValue = newVal;
		}
	},
	{
		immediate: true,
		flush: "post",
	}
);

watch(
	() => searchParam.value,
	(newVal) => {
		if (modelValue.value !== newVal?.searchValue) {
			modelValue.value = newVal.searchValue;
		}
	},
	{
		immediate: true,
		deep: true,
		flush: "post",
	}
);

onMounted(() => {
	defaultSearchTime();
});

defineExpose({
	/** 当前搜索参数 */
	searchParam: computed(() => searchParam.value),
});
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>
