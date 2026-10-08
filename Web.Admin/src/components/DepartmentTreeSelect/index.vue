<template>
	<FaTreeSelect
		v-bind="$attrs"
		:request-api="() => departmentApi.departmentSelector(props.orgId)"
		v-model="modelValue"
		v-model:label="departmentName"
		:placeholder="props.placeholder || '请选择部门'"
		check-strictly
		filterable
		clearable
		@change="(value) => emit('change', value)"
	/>
</template>

<script lang="ts" setup>
import { departmentApi } from "@/api/services/Admin/department";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "DepartmentTreeSelect",
});

const props = defineProps<{
	/** 查询部门时使用的机构 ID */
	orgId?: string;
	/** 选择框占位文本 */
	placeholder?: string;
}>();

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElSelectorOutput];
}>();

/** 选中的部门 ID，支持单值或多值 */
const modelValue = defineModel<string | string[]>();
/** 选中部门的名称 */
const departmentName = defineModel<string>("departmentName");
</script>
