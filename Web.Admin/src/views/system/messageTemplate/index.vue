<template>
	<div>
		<FaTable
			ref="faTableRef"
			row-key="recordId"
			:request-api="messageTemplateApi.queryMessageTemplatePaged"
			:columns="searchColumns"
			hide-search-time
		>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'MessageTemplate:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="tenantName" label="租户名称" fixed="left" width="240" small-width="220" sortable>
				<template #default="{ row }: { row: QueryMessageTemplatePagedOutput }">
					<span v-if="row.tenantName" v-iconCopy="row.tenantName">{{ row.tenantName }}</span>
					<el-tag v-else type="info">全局模板</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="templateType"
				label="模板类型"
				width="180"
				small-width="160"
				sortable
				tag
				:enum="appStore.getDictionary('MessageTemplateTypeEnum')"
			/>
			<FaTableColumn
				prop="templateCode"
				label="模板Code"
				width="300"
				small-width="280"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.recordId)"
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

			<template #operation="{ row }: { row: QueryMessageTemplatePagedOutput }">
				<el-button v-auth="'MessageTemplate:Detail'" size="small" plain @click="editFormRef.detail(row.recordId)">详情</el-button>
				<el-button v-auth="'MessageTemplate:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.recordId)">编辑</el-button>
				<el-button v-auth="'MessageTemplate:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<MessageTemplateEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script setup lang="ts">
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { messageTemplateApi } from "@/api/services/Center/messageTemplate";
import { useApp } from "@/stores";
import MessageTemplateEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryMessageTemplatePagedOutput } from "@/api/services/Center/messageTemplate/models/QueryMessageTemplatePagedOutput";

defineOptions({
	name: "SystemMessageTemplate",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof MessageTemplateEdit>>("editFormRef");

/** 处理删除 */
const handleDelete = (row: QueryMessageTemplatePagedOutput) => {
	const { recordId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除消息模板？", { type: "warning" }).then(async () => {
		await messageTemplateApi.deleteMessageTemplate({ recordId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "tenantId",
		label: "租户",
		search: { el: "TenantSelectPage" },
	},
];
</script>
