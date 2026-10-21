<template>
	<div>
		<FaTable row-key="recordId" :request-api="sqlDiffLogApi.querySqlDiffLogPaged" :columns="searchColumns">
			<FaTableColumn
				prop="mobile"
				label="昵称/手机"
				fixed="left"
				width="200"
				small-width="180"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: SqlDiffLogModel }">
					{{ row.nickName }}
					<br />
					手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="createdTime"
				label="执行时间"
				width="240"
				small-width="220"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: SqlDiffLogModel }">
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
			<FaTableColumn prop="os" label="设备" width="280" small-width="260" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: SqlDiffLogModel }">
					<span>设备：{{ row.device }}</span>
					<br />
					<span>操作系统：{{ row.os }}</span>
					<br />
					<span>浏览器：{{ row.browser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="tableName" label="表名称" width="200" small-width="180" sortable />
			<FaTableColumn prop="tableDescription" label="表描述" width="200" small-width="180" sortable />
			<FaTableColumn prop="diffType" label="差异类型" width="120" small-width="100" sortable tag :enum="diffLogTypeEnum" />
			<FaTableColumn prop="executeSeconds" label="执行秒数" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: SqlDiffLogModel }">
					{{ row.executeSeconds == null ? "--" : `${row.executeSeconds.toFixed(4)}秒` }}
				</template>
			</FaTableColumn>
			<FaTableColumn prop="beforeColumnList" label="旧的列信息" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: SqlDiffLogModel }">
					<el-tag
						v-if="row.beforeColumnList"
						type="info"
						style="cursor: pointer"
						@click="
							() => {
								state.title = '旧的列信息';
								state.content = JSON.stringify(row.beforeColumnList);
								state.visible = true;
							}
						"
					>
						查看
					</el-tag>
					<span v-else>--</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="afterColumnList" label="新的列信息" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: SqlDiffLogModel }">
					<el-tag
						v-if="row.afterColumnList"
						type="info"
						style="cursor: pointer"
						@click="
							() => {
								state.title = '新的列信息';
								state.content = JSON.stringify(row.afterColumnList);
								state.visible = true;
							}
						"
					>
						查看
					</el-tag>
					<span v-else>--</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="pureSql" label="纯Sql" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: SqlDiffLogModel }">
					<el-tag
						v-if="row.pureSql"
						type="info"
						style="cursor: pointer"
						@click="
							() => {
								state.title = '纯Sql';
								state.content = row.pureSql;
								state.visible = true;
							}
						"
					>
						查看
					</el-tag>
					<span v-else>--</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="tenantName" label="租户" width="200" small-width="180" sortable />
			<FaTableColumn prop="departmentName" label="部门" width="150" small-width="130" sortable />
		</FaTable>
		<el-dialog v-model="state.visible" :title="state.title" width="1000px" align-center draggable destroy-on-close>
			<el-scrollbar>
				<div style="max-height: 500px; padding-bottom: 20px; padding-right: 10px">
					<VueJsonPretty
						:data="jsonContent"
						:deep="3"
						show-length
						show-line-number
						show-icon
						virtual
						:height="500"
						:theme="configStore.layout.isDark ? 'dark' : 'light'"
					/>
				</div>
			</el-scrollbar>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive } from "vue";
import { dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import VueJsonPretty from "vue-json-pretty";
import { sqlDiffLogApi } from "@/api/services/Center/sqlDiffLog";
import { useApp, useConfig } from "@/stores";
import type { FaTableColumnCtx } from "fast-element-plus";
import type { JSONDataType } from "vue-json-pretty/types/utils";
import type { SqlDiffLogModel } from "@/api/services/Center/sqlDiffLog/models/SqlDiffLogModel";

defineOptions({
	name: "DevSqlDiffLog",
});

const appStore = useApp();
const diffLogTypeEnum = appStore.getDictionary("DiffLogTypeEnum");
const configStore = useConfig();

const state = reactive({
	visible: false,
	title: "日志",
	content: "",
});

const jsonContent = computed<JSONDataType>(() => {
	try {
		return JSON.parse(state.content) as JSONDataType;
	} catch {
		return state.content;
	}
});

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
		prop: "diffType",
		label: "差异类型",
		enum: diffLogTypeEnum,
		search: { el: "el-select" },
	},
];
</script>
