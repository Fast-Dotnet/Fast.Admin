<template>
	<FaCellPicker :title="props.title" :value="departmentName" :is-link="!props.disabled" placeholder="请选择部门">
		<template #default="{ visible }">
			<FaTreePicker
				title="部门选择"
				v-model="modelValue"
				v-model:label="departmentName"
				v-model:visible="visible.value"
				:request-api="() => departmentApi.departmentSelector(props.orgId)"
				clearable
				check-strictly
				disabled-search
				@change="(value, values) => emit('change', value, values)"
			/>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { departmentApi } from "@/api/services/Admin/department";

defineOptions({
	name: "DepartmentTreeSelect",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 标题 */
	title: {
		type: String,
		default: "部门",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
	/** 查询部门时使用的机构 ID */
	orgId: String,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [value: ElSelectorOutput<string> | null, values: ElSelectorOutput<string>[] | null];
}>();

/** 选中的部门 ID */
const modelValue = defineModel<string>();
/** 选中部门的名称 */
const departmentName = defineModel<string>("departmentName");
</script>
