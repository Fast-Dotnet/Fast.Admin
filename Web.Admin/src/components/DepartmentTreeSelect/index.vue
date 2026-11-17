<template>
	<FaTreeSelect
		v-bind="$attrs"
		ref="faTreeSelectRef"
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

<script setup lang="ts">
import { useTemplateRef, watch } from "vue";
import { departmentApi } from "@/api/services/Admin/department";
import type { ElSelectorOutput, FaTreeSelectInstance } from "fast-element-plus";

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

const faTreeSelectRef = useTemplateRef<FaTreeSelectInstance>("faTreeSelectRef");

/** 选中的部门 ID，支持单值或多值 */
const modelValue = defineModel<string | string[]>();
/** 选中部门的名称 */
const departmentName = defineModel<string | string[]>("departmentName");

watch(
	() => props.orgId,
	() => {
		faTreeSelectRef.value.refresh();
	}
);
</script>
