<template>
	<el-divider content-position="left">按钮信息</el-divider>
	<div style="height: 500px">
		<FaTable row-key="dictionaryItemId" :data="modelValue">
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button :disabled="props.disabled" type="primary" :icon="Plus" @click="editFormRef.add()">新增按钮</el-button>
			</template>
			<FaTableColumn prop="buttonName" label="按钮名称" width="120" small-width="100" />
			<FaTableColumn prop="buttonCode" label="按钮编码" width="200" small-width="200" />
			<FaTableColumn prop="edition" label="版本" width="120" small-width="120" tag :enum="editionEnum" />
			<FaTableColumn prop="hasWeb" label="Web端" width="80" small-width="80" tag :enum="booleanEnum" />
			<FaTableColumn prop="hasMobile" label="移动端" width="80" small-width="80" tag :enum="booleanEnum" />
			<FaTableColumn prop="hasDesktop" label="桌面端" width="80" small-width="80" tag :enum="booleanEnum" />
			<FaTableColumn prop="sort" label="排序" width="100" small-width="100" />
			<FaTableColumn prop="status" label="状态" width="100" small-width="80" tag :enum="commonStatusEnum" />
			<!-- 表格操作 -->
			<template #operation="{ row, $index }: { row: EditMenuButtonInput; $index: number }">
				<el-button size="small" plain @click="editFormRef.detail(row)">详情</el-button>
				<el-button :disabled="props.disabled" size="small" plain type="primary" @click="editFormRef.edit(row, $index)">编辑</el-button>
				<el-button :disabled="props.disabled" size="small" plain type="danger" @click="handleDelete(row, $index)">删除</el-button>
			</template>
		</FaTable>
	</div>
	<DevMenuEditButtonEdit ref="editFormRef" v-model="modelValue" />
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessageBox } from "element-plus";
import { useApp } from "@/stores";
import DevMenuEditButtonEdit from "./buttonEdit.vue";
import type { EditMenuButtonInput } from "@/api/services/Center/menu/models/EditMenuButtonInput";

defineOptions({
	name: "DevMenuEditButtonTable",
});

const props = defineProps({
	/** 是否禁用 */
	disabled: Boolean,
});

const appStore = useApp();
const editionEnum = appStore.getDictionary("EditionEnum");
const booleanEnum = appStore.getDictionary("BooleanEnum");
const commonStatusEnum = appStore.getDictionary("CommonStatusEnum");

// v-model绑定值
const modelValue = defineModel<EditMenuButtonInput[]>({ default: () => [], required: true });

const editFormRef = useTemplateRef<InstanceType<typeof DevMenuEditButtonEdit>>("editFormRef");

const handleDelete = (_row: EditMenuButtonInput, index: number) => {
	ElMessageBox.confirm("确定要删除按钮？", { type: "warning" }).then(() => {
		modelValue.value.splice(index, 1);
	});
};
</script>
