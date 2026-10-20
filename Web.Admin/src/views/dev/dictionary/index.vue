<template>
	<div>
		<FaTable ref="faTableRef" row-key="dictionaryId" :request-api="dictionaryApi.queryDictionaryPaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Dictionary:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn
				prop="dictionaryKey"
				label="字典Key"
				fixed="left"
				width="300"
				small-width="280"
				sortable
				copy
				link
				:click="({ row }) => editFormRef.detail(row.dictionaryId)"
			/>
			<FaTableColumn prop="dictionaryName" label="字典名称" width="300" small-width="280" sortable />
			<FaTableColumn
				prop="valueType"
				label="值类型"
				width="100"
				small-width="80"
				sortable
				tag
				:enum="[
					{ label: '字符串', value: 1, type: 'info' },
					{ label: 'Int', value: 2, type: 'success' },
					{ label: 'Long', value: 4, type: 'primary' },
					{ label: 'Boolean', value: 8, type: 'danger' },
				]"
			/>
			<FaTableColumn
				prop="hasFlags"
				label="Flags枚举"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="[
					{ label: '是', value: true, type: 'success' },
					{ label: '否', value: false, type: 'danger' },
				]"
			/>
			<FaTableColumn
				prop="status"
				label="状态"
				width="100"
				small-width="80"
				sortable
				tag
				:enum="[
					{ label: '正常', value: 1, type: 'primary' },
					{ label: '禁用', value: 2, type: 'danger' },
				]"
			/>
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" sortable />
			<FaTableColumn prop="createdTime" label="创建时间" type="timeInfo" width="240" small-width="220" sortable />
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
			<template #operation="{ row }: { row: QueryDictionaryPagedOutput }">
				<el-button v-auth="'Dictionary:Detail'" size="small" plain @click="editFormRef.detail(row.dictionaryId)">详情</el-button>
				<el-button v-auth="'Dictionary:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.dictionaryId)">编辑</el-button>
				<el-button v-auth="'Dictionary:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<DictionaryEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import DictionaryEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QueryDictionaryPagedOutput } from "@/api/services/Center/dictionary/models/QueryDictionaryPagedOutput";

defineOptions({
	name: "DevDictionary",
});

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof DictionaryEdit>>("editFormRef");

/** 处理删除 */
const handleDelete = (row: QueryDictionaryPagedOutput) => {
	const { dictionaryId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除数据字典？", { type: "warning" }).then(async () => {
		await dictionaryApi.deleteDictionary({ dictionaryId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};
</script>
