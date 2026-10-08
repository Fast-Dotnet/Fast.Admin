<template>
	<FaSelectPage
		v-bind="$attrs"
		:request-api="employeeApi.employeeSelector"
		v-model="modelValue"
		v-model:label="employeeName"
		placeholder="请选择职员"
		clearable
		more-detail
		@change="handleChange"
	>
		<template #default="data">
			<div style="display: flex; justify-content: space-between; align-items: center; gap: 8px; width: 100%">
				<FaAvatar :src="data.data?.idPhoto" thumb size="small" />
				<div style="flex: 1">
					<span>{{ data.label }}</span>
					<span style="display: flex; justify-content: space-between; width: 100%">
						<span style="font-size: var(--el-font-size-extra-small); padding-right: 8px">{{ data.data?.employeeNo }}</span>
						<span style="font-size: var(--el-font-size-extra-small)">{{ data.data?.mobile }}</span>
					</span>
				</div>
			</div>
		</template>
	</FaSelectPage>
</template>

<script lang="ts" setup>
import { employeeApi } from "@/api/services/Admin/employee";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "EmployeeSelectPage",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElSelectorOutput | undefined];
}>();

/** 选中的职员 ID */
const modelValue = defineModel<string>();
/** 选中职员的名称 */
const employeeName = defineModel<string>("employeeName");
/** 选中职员的工号；清空选择时同步清空 */
const employeeNo = defineModel<string>("employeeNo");
/** 选中职员的手机号；清空选择时同步清空 */
const mobile = defineModel<string>("mobile");

const handleChange = (data: ElSelectorOutput | ElSelectorOutput[]) => {
	if (Array.isArray(data)) return;
	if (data) {
		employeeNo.value = data.data?.employeeNo;
		mobile.value = data.data?.mobile;
		emit("change", data);
	} else {
		employeeNo.value = undefined;
		mobile.value = undefined;
		emit("change", undefined);
	}
};
</script>
