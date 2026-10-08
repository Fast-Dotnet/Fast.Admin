<template>
	<el-cascader
		v-bind="$attrs"
		v-model="modelValue"
		:options="state.regionList"
		placeholder="请选择地区"
		filterable
		clearable
		:props="{ checkOnClickNode: true, emitPath: true }"
		@change="handleChange"
	/>
</template>

<script lang="ts" setup>
import { onMounted, reactive } from "vue";
import { withDefineType } from "@fast-china/utils";
import { regionApi } from "@/api/services/Center/region";
import type { CascaderValue } from "element-plus";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "RegionSelect",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElSelectorOutput<string> | undefined];
}>();

/** 选中的区县编码；清空选择时为 null */
const modelValue = defineModel<string>();
/** 选中区县所属的省份名称 */
const provinceName = defineModel<string>("provinceName");
/** 选中区县所属的城市名称 */
const cityName = defineModel<string>("cityName");
/** 选中区县的名称 */
const districtName = defineModel<string>("districtName");

const state = reactive({
	regionList: withDefineType<ElSelectorOutput<string>[]>([]),
});

const handleChange = (val: CascaderValue) => {
	const value = val as string[];
	if (value && value.length > 0) {
		const provinceInfo = state.regionList.find((f) => f.value === value[0]);
		provinceName.value = provinceInfo.label;
		const cityInfo = provinceInfo?.children?.find((f) => f.value === value[1]);
		cityName.value = cityInfo?.label;
		const districtInfo = cityInfo?.children?.find((f) => f.value === value[2]);
		districtName.value = districtInfo?.label;
		modelValue.value = value[2];
		emit("change", districtInfo);
	} else {
		provinceName.value = null;
		cityName.value = null;
		districtName.value = null;
		modelValue.value = null;
		emit("change", undefined);
	}
};

onMounted(async () => {
	state.regionList = await regionApi.regionSelector();
});
</script>
