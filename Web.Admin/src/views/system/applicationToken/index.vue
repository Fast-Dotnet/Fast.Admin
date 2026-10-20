<template>
	<div>
		<FaTable ref="faTableRef" row-key="recordId" :request-api="applicationTokenApi.queryApplicationTokenPaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'AppToken:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="appKey" label="应用标识" fixed="left" width="240" small-width="220" copy sortable />
			<FaTableColumn prop="appName" label="应用名称" width="200" small-width="180" copy sortable />
			<FaTableColumn
				prop="appType"
				label="应用类型"
				width="150"
				small-width="130"
				sortable
				tag
				:enum="appStore.getDictionary('AppEnvironmentEnum')"
			/>
			<FaTableColumn
				prop="environmentType"
				label="环境类型"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('EnvironmentTypeEnum')"
			/>
			<FaTableColumn
				prop="tokenType"
				label="凭据类型"
				width="160"
				small-width="140"
				sortable
				tag
				:enum="appStore.getDictionary('ApplicationTokenTypeEnum')"
			/>
			<FaTableColumn prop="refreshTime" label="刷新时间" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="expireTime" label="过期时间" width="180" small-width="160" sortable type="dateTime" date-fix />

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryApplicationTokenPagedOutput }">
				<el-button v-auth="'AppToken:Refresh'" size="small" plain type="primary" @click="handleRefresh(row)">刷新</el-button>
				<el-button v-auth="'AppToken:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<ApplicationTokenEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script setup lang="ts">
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { applicationTokenApi } from "@/api/services/Center/applicationToken";
import { useApp } from "@/stores";
import ApplicationTokenEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QueryApplicationTokenPagedOutput } from "@/api/services/Center/applicationToken/models/QueryApplicationTokenPagedOutput";

defineOptions({
	name: "SystemApplicationToken",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof ApplicationTokenEdit>>("editFormRef");

/** 处理刷新 */
const handleRefresh = (row: QueryApplicationTokenPagedOutput) => {
	ElMessageBox.confirm("确定要刷新应用凭据？", { type: "warning" }).then(async () => {
		await applicationTokenApi.refreshApplicationToken({ recordId: row.recordId });
		ElMessage.success("刷新成功！");
		await faTableRef.value.refresh();
	});
};

/** 处理删除 */
const handleDelete = (row: QueryApplicationTokenPagedOutput) => {
	ElMessageBox.confirm("确定要删除应用凭据？", { type: "warning" }).then(async () => {
		await applicationTokenApi.deleteApplicationToken({ recordId: row.recordId });
		ElMessage.success("删除成功！");
		await faTableRef.value.refresh();
	});
};
</script>
