<template>
	<FaSelect
		v-bind="$attrs"
		:request-api="regionApi.provinceSelector"
		v-model="modelValue"
		v-model:label="provinceName"
		placeholder="请选择省份"
		more-detail
		@change="(data) => emit('change', data)"
	/>
</template>

<script lang="ts" setup>
import { regionApi } from "@/api/services/Center/region";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "ProvinceSelect",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElSelectorOutput];
}>();

/** 选中的省份编码 */
const modelValue = defineModel<string>();
/** 选中省份的名称 */
const provinceName = defineModel<string>("provinceName");
</script>
