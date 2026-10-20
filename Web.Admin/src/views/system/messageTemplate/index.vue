<template>
	<div>
		<FaTable ref="faTableRef" row-key="recordId" :request-api="messageTemplateApi.queryMessageTemplatePaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'MessageTemplate:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="appKey" label="应用标识" fixed="left" width="240" small-width="220" sortable>
				<template #default="{ row }: { row: QueryMessageTemplatePagedOutput }">
					<span v-if="row.appKey" v-iconCopy="row.appKey">{{ row.appKey }}</span>
					<el-tag v-else type="info">全局模板</el-tag>
				</template>
			</FaTableColumn>
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
import type { FaTableInstance } from "fast-element-plus";
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
</script>
