<template>
	<FaInputDialogPage
		v-bind="$attrs"
		row-key="employeeId"
		label-key="employeeName"
		:request-api="employeeApi.queryEmployeePaged"
		v-model="modelValue"
		v-model:label="employeeName"
		placeholder="请选择职员"
		@change="handleChange"
	>
		<FaTableColumn prop="idPhoto" label="头像" fixed type="image" width="50" small-width="50" />
		<FaTableColumn prop="employeeName" label="名称" fixed width="200" small-width="180" sortable />
		<FaTableColumn prop="employeeNo" label="工号" width="150" small-width="130" sortable />
		<FaTableColumn prop="mobile" label="手机" width="150" small-width="130" sortable />
		<FaTableColumn prop="departmentName" label="部门" width="150" small-width="130" sortable />
		<FaTableColumn prop="positionName" label="职位" width="150" small-width="130" sortable />
		<FaTableColumn prop="jobLevelName" label="职级" width="150" small-width="130" sortable />
	</FaInputDialogPage>
</template>

<script lang="ts" setup>
import { employeeApi } from "@/api/services/Admin/employee";
import type { QueryEmployeePagedOutput } from "@/api/services/Admin/employee/models/QueryEmployeePagedOutput";

defineOptions({
	name: "EmployeeDialogSelect",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [data: QueryEmployeePagedOutput | undefined];
}>();

/** 选中的职员 ID */
const modelValue = defineModel<string>();
/** 选中职员的名称 */
const employeeName = defineModel<string>("employeeName");
/** 选中职员的工号；清空选择时同步清空 */
const employeeNo = defineModel<string>("employeeNo");
/** 选中职员的手机号；清空选择时同步清空 */
const mobile = defineModel<string>("mobile");

const handleChange = (data: QueryEmployeePagedOutput) => {
	if (data) {
		employeeNo.value = data.employeeNo;
		mobile.value = data.mobile;
		emit("change", data);
	} else {
		employeeNo.value = undefined;
		mobile.value = undefined;
		emit("change", undefined);
	}
};
</script>
