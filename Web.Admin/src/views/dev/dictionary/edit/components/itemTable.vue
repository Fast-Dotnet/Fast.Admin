<template>
	<FaTable row-key="dictionaryItemId" :data="modelValue">
		<!-- 表格按钮操作区域 -->
		<template #header>
			<el-button :disabled="props.disabled" type="primary" :icon="Plus" @click="editFormRef.add()">新增字典项</el-button>
		</template>
		<FaTableColumn prop="label" label="字典项名称" width="120" small-width="100" />
		<FaTableColumn prop="value" label="字典项值" width="120" small-width="100" />
		<FaTableColumn
			prop="type"
			label="标签类型"
			width="100"
			small-width="80"
			tag
			:enum="[
				{ label: 'Primary', value: 1, type: 'primary' },
				{ label: 'Success', value: 2, type: 'success' },
				{ label: 'Info', value: 4, type: 'info' },
				{ label: 'Warning', value: 8, type: 'warning' },
				{ label: 'Danger', value: 16, type: 'danger' },
			]"
		/>
		<FaTableColumn prop="order" label="排序" width="100" small-width="80" />
		<FaTableColumn
			prop="visible"
			label="显示"
			width="100"
			small-width="80"
			tag
			:enum="[
				{ label: '显示', value: true, type: 'primary' },
				{ label: '隐藏', value: false, type: 'danger' },
			]"
		/>
		<FaTableColumn
			prop="status"
			label="状态"
			width="100"
			small-width="80"
			tag
			:enum="[
				{ label: '正常', value: 1, type: 'primary' },
				{ label: '禁用', value: 2, type: 'danger' },
			]"
		/>
		<FaTableColumn prop="tips" label="提示" width="120" small-width="100" sortable />
		<!-- 表格操作 -->
		<template #operation="{ row, $index }: { row: EditDictionaryItemInput; $index: number }">
			<el-button size="small" plain @click="editFormRef.detail(row)">详情</el-button>
			<el-button :disabled="props.disabled" size="small" plain type="primary" @click="editFormRef.edit(row, $index)">编辑</el-button>
			<el-button :disabled="props.disabled" size="small" plain type="danger" @click="handleDelete(row, $index)">删除</el-button>
		</template>
	</FaTable>
	<DictionaryEditItemEdit ref="editFormRef" v-model="modelValue" />
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessageBox } from "element-plus";
import DictionaryEditItemEdit from "./itemEdit.vue";
import type { EditDictionaryItemInput } from "@/api/services/Center/dictionary/models/EditDictionaryItemInput";

defineOptions({
	name: "DevDictionaryEditItemTable",
});

const props = defineProps({
	/** 是否禁用 */
	disabled: Boolean,
});

// v-model绑定值
const modelValue = defineModel<EditDictionaryItemInput[]>({ required: true });

const editFormRef = useTemplateRef<InstanceType<typeof DictionaryEditItemEdit>>("editFormRef");

const handleDelete = (_row: EditDictionaryItemInput, index: number) => {
	ElMessageBox.confirm("确定要删除数据字典项？", {
		type: "warning",
	}).then(() => {
		modelValue.value.splice(index, 1);
	});
};
</script>
