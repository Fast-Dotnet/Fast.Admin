<template>
	<div>
		<FaTable ref="faTableRef" row-key="jobLevelId" :request-api="jobLevelApi.queryJobLevelPaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'JobLevel:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn
				prop="jobLevelName"
				label="职级名称"
				fixed="left"
				width="200"
				small-width="180"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.jobLevelId)"
			/>
			<FaTableColumn prop="level" label="等级" width="100" small-width="80" sortable />
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
			<template #operation="{ row }: { row: QueryJobLevelPagedOutput }">
				<el-button v-auth="'JobLevel:Detail'" size="small" plain @click="editFormRef.detail(row.jobLevelId)">详情</el-button>
				<el-button v-auth="'JobLevel:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.jobLevelId)">编辑</el-button>
				<el-button v-auth="'JobLevel:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<JobLevelEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import JobLevelEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QueryJobLevelPagedOutput } from "@/api/services/Admin/jobLevel/models/QueryJobLevelPagedOutput";

defineOptions({
	name: "AdminJobLevel",
});

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof JobLevelEdit>>("editFormRef");

/** 处理删除 */
const handleDelete = (row: QueryJobLevelPagedOutput) => {
	const { jobLevelId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除职级？", { type: "warning" }).then(async () => {
		await jobLevelApi.deleteJobLevel({ jobLevelId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};
</script>
