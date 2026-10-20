<template>
	<div>
		<FaTable v-show="!state.isEdit" ref="faTableRef" row-key="tableId" :request-api="tableApi.queryTableConfigPaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Table:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn
				prop="tableKey"
				label="表格Key"
				fixed="left"
				width="200"
				small-width="180"
				sortable
				copy
				link
				:click="({ row }) => editFormRef.detail(row.tableId)"
			/>
			<FaTableColumn prop="tableName" label="表格名称" width="400" small-width="380" sortable />
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" sortable />
			<FaTableColumn prop="createdTime" label="创建时间" type="timeInfo" width="240" small-width="220" sortable />
			<FaTableColumn
				prop="updatedTime"
				label="更新时间"
				type="timeInfo"
				width="240"
				small-width="220"
				sortable
				:time-info-field="{
					userName: 'updatedUserName',
					time: 'updatedTime',
				}"
			/>
			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryTableConfigPagedOutput }">
				<el-button v-auth="'Table:Detail'" size="small" plain @click="editFormRef.detail(row.tableId)">详情</el-button>
				<el-button v-auth="'Table:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.tableId)">编辑</el-button>
				<el-button v-auth="'Table:Edit'" size="small" plain type="info" @click="editFormRef.copy(row.tableId)">复制</el-button>
				<el-button v-auth="'Table:Edit'" size="small" plain type="success" @click="handleColumnConfigClick(row)">配置列</el-button>
				<el-button v-auth="'Table:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<TableColumnConfig v-show="state.isEdit" ref="tableColumnConfigRef" @back="handleBack" @ok="faTableRef.refresh()" />
		<TableConfigEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { tableApi } from "@/api/services/Center/table";
import TableColumnConfig from "./config/index.vue";
import TableConfigEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QueryTableConfigPagedOutput } from "@/api/services/Center/table/models/QueryTableConfigPagedOutput";

defineOptions({
	name: "DevTableConfig",
});

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof TableConfigEdit>>("editFormRef");
const tableColumnConfigRef = useTemplateRef<InstanceType<typeof TableColumnConfig>>("tableColumnConfigRef");

const state = reactive({
	/** 是否编辑 */
	isEdit: false,
});

/** 处理配置列点击 */
const handleColumnConfigClick = (row: QueryTableConfigPagedOutput) => {
	tableColumnConfigRef.value.edit(row.tableId, row.tableName, row.rowVersion);
	state.isEdit = true;
};

/** 处理返回 */
const handleBack = () => {
	state.isEdit = false;
};

/** 处理删除 */
const handleDelete = (row: QueryTableConfigPagedOutput) => {
	const { tableId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除表格配置？", { type: "warning" }).then(async () => {
		await tableApi.deleteTableConfig({ tableId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};
</script>
