<template>
	<FaDialog
		ref="faDialogRef"
		width="1200"
		full-height
		:title="state.dialogTitle"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<div class="fa__display_tb-b">
			<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="2">
				<FaFormItem prop="dictionaryKey" label="字典Key">
					<el-text type="primary">{{ state.formData.dictionaryKey }}</el-text>
				</FaFormItem>
				<FaFormItem prop="dictionaryName" label="字典名称">
					<el-input v-model="state.formData.dictionaryName" maxlength="50" placeholder="请输入字典名称" />
				</FaFormItem>
				<FaFormItem prop="valueType" label="值类型">
					<el-radio-group v-model="state.formData.valueType">
						<el-radio :value="1">字符串</el-radio>
						<el-radio :value="2">数字（Int）</el-radio>
						<el-radio :value="4">数字（Long）</el-radio>
						<el-radio :value="8">Boolean</el-radio>
					</el-radio-group>
				</FaFormItem>
				<FaFormItem prop="status" label="状态">
					<el-radio-group v-model="state.formData.status">
						<el-radio :value="1">正常</el-radio>
						<el-radio :value="2">禁用</el-radio>
					</el-radio-group>
				</FaFormItem>
				<FaFormItem prop="remark" label="备注">
					<el-input v-model="state.formData.remark" type="textarea" maxlength="200" placeholder="请输入备注" />
				</FaFormItem>
			</FaForm>
			<ItemTable v-model="state.formData.dictionaryItemList" />
		</div>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import ItemTable from "./components/itemTable.vue";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { EditDictionaryInput } from "@/api/services/Center/dictionary/models/EditDictionaryInput";
import type { QueryDictionaryDetailOutput } from "@/api/services/Center/dictionary/models/QueryDictionaryDetailOutput.ts";

defineOptions({
	name: "DevDictionaryEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditDictionaryInput & QueryDictionaryDetailOutput>({}),
	formRules: withDefineType<FormRules<EditDictionaryInput>>({
		dictionaryName: [{ required: true, message: "请输入字典名称", trigger: "blur" }],
	}),
	dialogTitle: "数据字典",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await dictionaryApi.editDictionary(state.formData);
		ElMessage.success("保存成功！");
		emit("ok");
	});
};

const edit = (dictionaryId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await dictionaryApi.queryDictionaryDetail(dictionaryId);
		state.formData = apiRes;
		state.dialogTitle = `编辑数据字典 - ${apiRes.dictionaryName}`;
	});
};

defineExpose({
	element: faDialogRef,
	edit,
});
</script>
