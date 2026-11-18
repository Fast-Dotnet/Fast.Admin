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

<script setup lang="ts">
import { regionApi } from "@/api/services/Center/region";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "ProvinceSelect",
});

const emit = defineEmits<{
	/** 选项变化时透传基础选择组件的结果 */
	change: [value: ElSelectorOutput];
}>();

/** 选中的省份 ID */
const modelValue = defineModel<string>();
/** 选中省份的名称 */
const provinceName = defineModel<string>("provinceName");
</script>
