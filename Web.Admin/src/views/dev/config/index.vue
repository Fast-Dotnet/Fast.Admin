<template>
	<div>
		<FaTable ref="faTableRef" row-key="configId" :request-api="configApi.queryConfigPaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Config:Edit'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
				<el-button v-auth="'Config:Edit'" plain type="warning" :icon="Delete" @click="handleDeleteAll">删除全部缓存</el-button>
			</template>

			<FaTableColumn
				prop="configCode"
				label="配置编码"
				fixed="left"
				width="300"
				small-width="280"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.configId)"
			/>
			<FaTableColumn prop="configName" label="配置名称" width="200" small-width="180" sortable />
			<FaTableColumn prop="configValue" label="配置值" width="300" small-width="280" sortable />
			<FaTableColumn prop="remark" label="备注" width="500" small-width="480" sortable />
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
			<template #operation="{ row }: { row: QueryConfigPagedOutput }">
				<el-button v-auth="'Config:Detail'" size="small" plain @click="editFormRef.detail(row.configId)">详情</el-button>
				<el-button v-auth="'Config:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.configId)">编辑</el-button>
				<el-button v-auth="'Config:Edit'" size="small" plain type="warning" @click="handleDelete(row)">删除缓存</el-button>
			</template>
		</FaTable>
		<ConfigEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Delete, Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { configApi } from "@/api/services/Center/config";
import ConfigEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QueryConfigPagedOutput } from "@/api/services/Center/config/models/QueryConfigPagedOutput";

defineOptions({
	name: "DevConfig",
});

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof ConfigEdit>>("editFormRef");

/** 处理删除缓存 */
const handleDelete = (row: QueryConfigPagedOutput) => {
	const { configCode } = row;
	ElMessageBox.confirm("确定要删除缓存？", { type: "warning" }).then(async () => {
		await configApi.deleteConfigCache({ configCode });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};

/** 处理删除全部缓存 */
const handleDeleteAll = () => {
	ElMessageBox.confirm("确定要删除全部缓存？", { type: "warning" }).then(async () => {
		await configApi.deleteAllConfigCache();
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};
</script>
