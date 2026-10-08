<template>
	<FaTable :data="modelValue" :pagination="false">
		<!-- 表格按钮操作区域 -->
		<template #header>
			<el-button type="primary" :icon="Plus" @click="handleTableRowAdd">新增字段</el-button>
		</template>
		<FaTableColumn prop="prop" label="字段名称" width="280">
			<template #default="{ row }: { row: FaTableColumnAdvancedCtx }">
				<el-input v-model="row.prop" maxlength="50" placeholder="请输入字段名称" />
			</template>
		</FaTableColumn>
		<FaTableColumn prop="type" label="字段类型" width="280">
			<template #default="{ row }: { row: FaTableColumnAdvancedCtx }">
				<el-radio-group v-model="row.type">
					<el-radio :value="1">字符串</el-radio>
					<el-radio :value="2">数字</el-radio>
					<el-radio :value="4">Boolean</el-radio>
					<el-radio :value="8">Function</el-radio>
				</el-radio-group>
			</template>
		</FaTableColumn>
		<FaTableColumn prop="value" label="字段值" width="280">
			<template #default="{ row }: { row: FaTableColumnAdvancedCtx }">
				<el-input v-model="row.value" type="textarea" maxlength="500" placeholder="请输入字段值" />
			</template>
		</FaTableColumn>
		<!-- 表格操作 -->
		<template #operation="{ $index }">
			<el-button size="small" plain type="danger" @click="handleTableRowDelete($index)">删除字段</el-button>
		</template>
	</FaTable>
</template>

<script lang="ts" setup>
import { Plus } from "@element-plus/icons-vue";
import type { FaTableColumnAdvancedCtx } from "@/api/services/Center/table/models/FaTableColumnAdvancedCtx";

defineOptions({
	name: "DevTableConfigObjectTable",
});

// v-model绑定值
const modelValue = defineModel<FaTableColumnAdvancedCtx[]>({ required: true });

/** 处理新增行。 */
const handleTableRowAdd = () => {
	modelValue.value.push({ type: 1 });
};

/** 处理删除行。 */
const handleTableRowDelete = (index: number) => {
	modelValue.value.splice(index, 1);
};
</script>
