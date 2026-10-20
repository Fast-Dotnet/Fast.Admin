<template>
	<div>
		<FaTable ref="faTableRef" row-key="mainId" :request-api="databaseApi.queryDatabasePaged" :columns="searchColumns" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Database:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="tenantName" label="租户名称" fixed="left" width="200" small-width="180" sortable />
			<FaTableColumn
				prop="databaseType"
				label="数据库类型"
				fixed="left"
				width="150"
				small-width="130"
				sortable
				tag
				:enum="appStore.getDictionary('DatabaseTypeEnum')"
			/>
			<FaTableColumn prop="dbType" label="Db类型" width="150" small-width="130" sortable tag :enum="appStore.getDictionary('SugarDbType')" />
			<FaTableColumn prop="publicIp" label="公网Ip地址" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="intranetIp" label="内网Ip地址" width="150" small-width="130" sortable />
			<FaTableColumn prop="port" label="端口号" width="100" small-width="80" sortable />
			<FaTableColumn
				prop="dbName"
				label="数据库名称"
				width="200"
				small-width="180"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.mainId)"
			/>
			<FaTableColumn prop="dbUser" label="数据库用户" width="150" small-width="130" sortable />
			<FaTableColumn prop="commandTimeOut" label="超时时间" width="120" small-width="100" sortable tag>
				<template #default="{ row }: { row: QueryDatabasePagedOutput }">
					<el-tag>{{ row.commandTimeOut }}秒</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="sugarSqlExecMaxSeconds" label="警告时间" width="120" small-width="100" sortable tag>
				<template #default="{ row }: { row: QueryDatabasePagedOutput }">
					<el-tag>{{ row.sugarSqlExecMaxSeconds }}秒</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="diffLog" label="差异日志" width="120" small-width="100" sortable tag :enum="appStore.getDictionary('BooleanEnum')" />
			<FaTableColumn
				prop="disableAop"
				label="禁用Aop"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('BooleanEnum')"
			/>
			<FaTableColumn
				prop="isInitialized"
				label="初始化"
				width="100"
				small-width="80"
				sortable
				tag
				:enum="appStore.getDictionary('BooleanEnum')"
			/>
			<FaTableColumn prop="createdTime" label="创建时间" width="240" small-width="220" sortable type="timeInfo" />
			<FaTableColumn
				prop="updatedTime"
				label="更新时间"
				width="240"
				small-width="220"
				sortable
				type="timeInfo"
				:time-info-field="{ userName: 'updatedUserName', time: 'updatedTime' }"
			/>

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryDatabasePagedOutput }">
				<el-button v-auth="'Database:Detail'" size="small" plain @click="editFormRef.detail(row.mainId)">详情</el-button>
				<el-button v-auth="'Database:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.mainId)">编辑</el-button>
				<el-button v-auth="'Database:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
				<el-button v-if="!row.isInitialized" v-auth="'Database:Edit'" size="small" plain type="warning" @click="handleInitDatabase(row)">
					初始化
				</el-button>
			</template>
		</FaTable>
		<DatabaseEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { databaseApi } from "@/api/services/Center/database";
import { useApp } from "@/stores";
import DatabaseEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryDatabasePagedOutput } from "@/api/services/Center/database/models/QueryDatabasePagedOutput";

defineOptions({
	name: "SystemDatabase",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof DatabaseEdit>>("editFormRef");

/** 处理删除 */
const handleDelete = (row: QueryDatabasePagedOutput) => {
	const { mainId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除数据库？", { type: "warning" }).then(async () => {
		await databaseApi.deleteDatabase({ mainId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};

/** 处理初始化 */
const handleInitDatabase = (row: QueryDatabasePagedOutput) => {
	const { tenantId, databaseType, isInitialized } = row;
	if (isInitialized) return;
	ElMessageBox.confirm("确定要初始化数据库？", { type: "warning" }).then(async () => {
		await databaseApi.initDatabase({ tenantId, databaseType });
		ElMessage.success("初始化成功！");
		await faTableRef.value?.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "tenantId",
		label: "租户名称",
		search: { el: "TenantSelectPage" },
	},
	{
		pureSearch: true,
		prop: "databaseType",
		label: "数据库类型",
		enum: appStore.getDictionary("DatabaseTypeEnum"),
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "dbType",
		label: "Db类型",
		enum: appStore.getDictionary("SugarDbType"),
		search: { el: "el-select" },
	},
];
</script>
