<template>
	<div>
		<FaTable row-key="recordId" :request-api="operateLogApi.queryOperateLogPaged" :columns="searchColumns" data-search-range="Past1M">
			<FaTableColumn
				prop="employeeNo"
				label="姓名/工号"
				fixed="left"
				width="200"
				small-width="180"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: OperateLogModel }">
					{{ row.createdUserName }}
					<br />
					工号：<span v-iconCopy="row.employeeNo">{{ row.employeeNo }}</span>
					<br />
					手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="createdTime"
				label="操作时间"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: OperateLogModel }">
					<span>地区：{{ row.province }} - {{ row.city }}</span>
					<br />
					<span>Ip：{{ row.ip }}</span>
					<br />
					<span>时间：{{ dayjs(row.createdTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					<el-tag v-if="row.createdTime" type="info" round effect="light" size="small" style="margin-left: 5px">
						{{ formatChineseRelativeTime(row.createdTime) }}
					</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="os" label="操作设备" width="280" small-width="260" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: OperateLogModel }">
					<span>设备：{{ row.device }}</span>
					<br />
					<span>操作系统：{{ row.os }}</span>
					<br />
					<span>浏览器：{{ row.browser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="operateType"
				label="类型"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="operateLogTypeEnum"
				:show-overflow-tooltip="false"
			/>
			<FaTableColumn prop="title" label="标题" width="200" small-width="180" sortable />
			<FaTableColumn prop="bizNo" label="业务编码" width="200" small-width="180" sortable />
			<FaTableColumn prop="description" label="描述" width="500" small-width="480" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="departmentName" label="部门" width="150" small-width="130" sortable />
		</FaTable>
	</div>
</template>

<script lang="ts" setup>
import { dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { operateLogApi } from "@/api/services/Admin/operateLog";
import { useApp } from "@/stores";
import type { FaTableColumnCtx } from "fast-element-plus";
import type { OperateLogModel } from "@/api/services/Admin/operateLog/models/OperateLogModel";

defineOptions({
	name: "AdminOperateLog",
});

const appStore = useApp();
const operateLogTypeEnum = appStore.getDictionary("OperateLogTypeEnum");

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "employeeId",
		label: "职员",
		search: { el: "EmployeeSelectPage" },
	},
	{
		pureSearch: true,
		prop: "operateType",
		label: "类型",
		enum: operateLogTypeEnum,
		search: { el: "el-select" },
	},
];
</script>
