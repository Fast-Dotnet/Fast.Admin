<template>
	<div>
		<FaTable row-key="recordId" :request-api="visitLogApi.queryVisitLogPaged" :columns="searchColumns" data-search-range="Past1M">
			<FaTableColumn
				prop="mobile"
				label="昵称/手机"
				fixed="left"
				width="200"
				small-width="180"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: VisitLogModel }">
					{{ row.nickName }}
					<br />
					手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="createdTime"
				label="访问时间"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: VisitLogModel }">
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
			<FaTableColumn prop="os" label="访问设备" width="280" small-width="260" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: VisitLogModel }">
					<span>设备：{{ row.device }}</span>
					<br />
					<span>操作系统：{{ row.os }}</span>
					<br />
					<span>浏览器：{{ row.browser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="visitType"
				label="类型"
				width="100"
				small-width="80"
				sortable
				tag
				:enum="visitTypeEnum"
				:show-overflow-tooltip="false"
			/>
			<FaTableColumn prop="tenantName" label="租户" width="200" small-width="180" sortable />
			<FaTableColumn prop="departmentName" label="部门" width="150" small-width="130" sortable />
		</FaTable>
	</div>
</template>

<script lang="ts" setup>
import { dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { visitLogApi } from "@/api/services/Center/visitLog";
import { useApp } from "@/stores";
import type { FaTableColumnCtx } from "fast-element-plus";
import type { VisitLogModel } from "@/api/services/Center/visitLog/models/VisitLogModel";

defineOptions({
	name: "SystemVisitLog",
});

const appStore = useApp();
const visitTypeEnum = appStore.getDictionary("VisitTypeEnum");
const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "accountId",
		label: "账号",
		search: { el: "AccountSelectPage" },
	},
	{
		pureSearch: true,
		prop: "tenantId",
		label: "租户",
		search: { el: "TenantSelectPage" },
	},
	{
		pureSearch: true,
		prop: "visitType",
		label: "类型",
		enum: visitTypeEnum,
		search: { el: "el-select" },
	},
];
</script>
