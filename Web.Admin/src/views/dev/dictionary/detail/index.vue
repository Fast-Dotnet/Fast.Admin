<template>
	<FaDialog ref="faDialogRef" width="1200" full-height :title="state.dialogTitle" :show-confirm-button="false">
		<div class="fa__display_tb-b">
			<FaForm :model="state.formData" detail-form cols="2">
				<FaFormItem label="字典Key" :value="state.formData.dictionaryKey" />
				<FaFormItem label="字典名称" :value="state.formData.dictionaryName" />
				<FaFormItem label="值类型">
					<el-tag v-if="state.formData.valueType === 1" type="primary">字符串</el-tag>
					<el-tag v-if="state.formData.valueType === 2" type="primary">数字（Int）</el-tag>
					<el-tag v-if="state.formData.valueType === 4" type="primary">数字（Long）</el-tag>
					<el-tag v-if="state.formData.valueType === 8" type="primary">Boolean</el-tag>
				</FaFormItem>
				<FaFormItem label="状态">
					<el-tag v-if="state.formData.status === 1" type="primary">正常</el-tag>
					<el-tag v-if="state.formData.status === 2" type="danger">禁用</el-tag>
				</FaFormItem>
				<FaFormItem label="备注" :value="state.formData.remark" />
			</FaForm>

			<FaTable row-key="dictionaryItemId" :data="state.formData.dictionaryItemList" :header-card="false">
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
			</FaTable>
		</div>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryDictionaryDetailOutput } from "@/api/services/Center/dictionary/models/QueryDictionaryDetailOutput.ts";

defineOptions({
	name: "DevDictionaryDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryDictionaryDetailOutput>({}),
	dialogTitle: "数据字典",
});

const detail = (dictionaryId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await dictionaryApi.queryDictionaryDetail(dictionaryId);
		state.formData = apiRes;
		state.dialogTitle = `数据字典详情 - ${apiRes.dictionaryName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
