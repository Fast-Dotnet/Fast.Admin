<template>
	<FaCellPicker
		v-bind="$attrs"
		:title="props.title"
		:value="props.multiple ? `已选 ${modelValue.length} 个部门` : departmentName ? String(departmentName) : undefined"
		:is-link="!props.disabled"
		placeholder="请选择部门"
	>
		<template #default="{ visible }">
			<FaSelectPage
				v-if="props.multiple"
				title="部门选择"
				v-model="modelValue"
				v-model:label="departmentName"
				v-model:visible="visible.value"
				:data="departmentList"
				:open-callback="handleOpenCallback"
				:pagination="false"
				disabled-search
				multiple
				tree-select
				clearable
				@change="(value) => emit('change', value, null)"
			>
				<template #default="{ item }">
					{{ item.treeLabel }}
				</template>
			</FaSelectPage>
			<FaTreePicker
				v-else
				title="部门选择"
				:model-value="Array.isArray(modelValue) ? undefined : modelValue"
				:label="Array.isArray(departmentName) ? undefined : departmentName"
				v-model:visible="visible.value"
				:request-api="() => departmentApi.departmentSelector(props.orgId)"
				disabled-search
				clearable
				check-strictly
				@update:model-value="(value) => (modelValue = value)"
				@update:label="(value) => (departmentName = value)"
				@change="(value, values) => emit('change', value, values)"
			/>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { shallowRef } from "vue";
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
	/** 选择入口的标题 */
	title: {
		type: String,
		default: "部门",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
	/** 是否使用多选分组列表；关闭时使用逐级单选 */
	multiple: Boolean,
	/** 查询部门时使用的机构 ID，每次打开选择器时读取 */
	orgId: String,
});

const emit = defineEmits<{
	/** 单选返回节点及完整路径；多选确认返回选项数组和 null，点击清空时两个参数均为 null */
	change: [value: ElSelectorOutput<string> | ElSelectorOutput<string>[] | null, values: ElSelectorOutput<string>[] | null];
}>();

/** 单选时为部门 ID，多选时为部门 ID 数组 */
const modelValue = defineModel<string | string[]>();
/** 单选时为部门名称，多选时为选中项标签数组 */
const departmentName = defineModel<string | string[]>("departmentName");

const departmentList = shallowRef<ElSelectorOutput<string>[]>([]);

const flattenDepartments = (items: ElSelectorOutput<string>[], parentLabel = ""): ElSelectorOutput<string>[] =>
	items.flatMap(({ children, ...item }) => {
		const treeLabel = [parentLabel, item.label].filter(Boolean).join(" / ");
		return [{ ...item, treeLabel }, ...flattenDepartments(children ?? [], treeLabel)];
	});

const handleOpenCallback = async () => {
	const apiRes = await departmentApi.departmentSelector(props.orgId);
	departmentList.value = apiRes.map((item) => ({
		...item,
		children: flattenDepartments([item]),
	}));
};
</script>
