<template>
	<view class="fa-search-input">
		<view class="fa-search-input__header">
			<slot name="prefix" />
			<wd-input
				v-model="modelValue"
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
			<view v-if="props.filter" class="fa-search-input__filter" @click="state.filterVisible = true">
				<FaIcon custom-class="wd-input__icon" name="filter" />
				<text class="fa-search-input__filter-text">筛选</text>
			</view>
			<slot name="suffix" />
		</view>
		<view class="fa-search-input__footer">
			<slot name="footer" />
		</view>
		<wd-popup v-if="props.filter" custom-class="fa-search-popup" v-model="state.filterVisible" closable position="bottom" safe-area-inset-bottom>
			<text class="fa-search-popup__title">高级筛选</text>
			<view class="fa-search-popup__warp">
				<template v-if="!props.hideSearchTime">
					<wd-datetime-picker
						:disable="state.dataSearchRange !== 'custom'"
						type="date"
						label="时间"
						v-model="state.searchTimeList"
						:default-value="createOneMonthRangeFromToday(props.futureSearchTime).map((date) => date.getTime())"
						align-right
						@confirm="handleDateTimeConfirm"
					/>
					<wd-radio-group
						class="fa-search-input__date-range"
						v-model="state.dataSearchRange"
						cell
						shape="button"
						@change="handleDateRangeChange"
					>
						<view class="fa-search-input__date-range__warp">
							<wd-radio v-for="(item, index) in dataSearchRangeList" :key="index" :value="item.value">
								{{ item.label }}
							</wd-radio>
							<wd-radio value="custom">自定义</wd-radio>
						</view>
					</wd-radio-group>
				</template>
				<slot name="filter" />
			</view>
			<view class="fa-search-popup__footer">
				<wd-button type="info" plain block @click="state.filterVisible = false">取消</wd-button>
				<wd-button type="primary" block @click="handleFilterClick">筛选</wd-button>
			</view>
		</wd-popup>
	</view>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, watch } from "vue";
import { createOneMonthRangeFromToday, withDefineType } from "@fast-china/utils";
import dayjs from "dayjs";
import { useConfig } from "@/stores";

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
	/** 隐藏搜索时间 */
	hideSearchTime: Boolean,
	/** 是否使用从今天起向未来延伸的快捷日期范围 */
	futureSearchTime: Boolean,
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

const configStore = useConfig();

/** 搜索文本，与 searchParam.searchValue 双向同步 */
const modelValue = defineModel<string>();
/** 分页搜索参数，包含搜索文本和格式化后的日期范围 */
const searchParam = defineModel<PagedInput>("searchParam", {
	default: () => ({}),
});

const state = reactive({
	filterVisible: false,
	searchTimeList: withDefineType<number[]>([]),
	dataSearchRange: withDefineType<FaTableDataRange | "custom">(),
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

const handleScanClick = () => {
	uni.scanCode({
		onlyFromCamera: true,
		scanType: ["barCode", "qrCode", "datamatrix", "pdf417"],
		success: (res) => {
			modelValue.value = res.result;
			searchParam.value.searchValue = res.result;
		},
	});
};

const handleInput = (detail: UniHelper.InputOnInputDetail) => {
	searchParam.value.searchValue = detail.value;
	emit("input", detail);
};

const handleClear = () => {
	searchParam.value.searchValue = "";
	emit("clear");
};

const handleFilterClick = () => {
	state.filterVisible = false;
	emit("search");
};

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
	searchParam.value.searchTimeList = [dayjs(value[0]).format("YYYY-MM-DD 00:00:00"), dayjs(value[1]).format("YYYY-MM-DD 23:59:59")];
};

const handleDateRangeChange = ({ value }: { value: FaTableDataRange | "custom" }) => {
	const { start, end } = getDateTime(value);
	state.searchTimeList = [start.getTime(), end.getTime()];
	searchParam.value.searchTimeList = [dayjs(start).format("YYYY-MM-DD 00:00:00"), dayjs(end).format("YYYY-MM-DD 23:59:59")];
};

const defaultSearchTime = () => {
	if (!props.filter || props.hideSearchTime) {
		state.searchTimeList = [];
	} else {
		const { start, end } = getDateTime(configStore.tableLayout.dataSearchRange);
		state.dataSearchRange = configStore.tableLayout.dataSearchRange;
		state.searchTimeList = [start.getTime(), end.getTime()];
		searchParam.value.searchTimeList = [dayjs(start).format("YYYY-MM-DD 00:00:00"), dayjs(end).format("YYYY-MM-DD 23:59:59")];
	}
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
	}
);

onMounted(() => {
	defaultSearchTime();
});
</script>

<style scoped lang="scss">
@use "./index.scss";
</style>
