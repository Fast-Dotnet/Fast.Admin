<template>
	<div>
		<FaTable ref="faTableRef" row-key="positionId" :request-api="positionApi.queryPositionPaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Position:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn
				prop="positionName"
				label="职位名称"
				fixed="left"
				width="200"
				small-width="180"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.positionId)"
			/>
			<FaTableColumn prop="sort" label="排序" width="100" small-width="80" sortable />
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" sortable />
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
			<template #operation="{ row }: { row: QueryPositionPagedOutput }">
				<el-button v-auth="'Position:Detail'" size="small" plain @click="editFormRef.detail(row.positionId)">详情</el-button>
				<el-button v-auth="'Position:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.positionId)">编辑</el-button>
				<el-button v-auth="'Position:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<PositionEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { positionApi } from "@/api/services/Admin/position";
import PositionEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QueryPositionPagedOutput } from "@/api/services/Admin/position/models/QueryPositionPagedOutput";

defineOptions({
	name: "AdminPosition",
});

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof PositionEdit>>("editFormRef");

/** 处理删除 */
const handleDelete = (row: QueryPositionPagedOutput) => {
	const { positionId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除职位？", { type: "warning" }).then(async () => {
		await positionApi.deletePosition({ positionId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};
</script>
