<template>
	<FaDialog
		ref="faDialogRef"
		width="600"
		title="添加数据字典"
		:show-fullscreen="false"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="1">
			<FaFormItem prop="dictionaryKey" label="字典Key">
				<el-input v-model="state.formData.dictionaryKey" maxlength="50" placeholder="请输入字典Key" />
			</FaFormItem>
			<FaFormItem prop="dictionaryName" label="字典名称">
				<el-input v-model="state.formData.dictionaryName" maxlength="50" placeholder="请输入字典名称" />
			</FaFormItem>
			<FaFormItem prop="valueType" label="值类型" span="2">
				<el-radio-group v-model="state.formData.valueType">
					<el-radio :value="1">字符串</el-radio>
					<el-radio :value="2">数字（Int）</el-radio>
					<el-radio :value="4">数字（Long）</el-radio>
					<el-radio :value="8">Boolean</el-radio>
				</el-radio-group>
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input v-model="state.formData.remark" type="textarea" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { DictionaryValueTypeEnum } from "@/api/enums/DictionaryValueTypeEnum";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddDictionaryInput } from "@/api/services/Center/dictionary/models/AddDictionaryInput";

defineOptions({
	name: "DevDictionaryAdd",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<AddDictionaryInput>({}),
	formRules: withDefineType<FormRules<AddDictionaryInput>>({
		dictionaryKey: [{ required: true, message: "请输入字典Key", trigger: "blur" }],
		dictionaryName: [{ required: true, message: "请输入字典名称", trigger: "blur" }],
	}),
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await dictionaryApi.addDictionary(state.formData);
		ElMessage.success("新增成功！");
		emit("ok");
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.formData = {
			valueType: DictionaryValueTypeEnum.String,
		};
	});
};

defineExpose({
	element: faDialogRef,
	add,
});
</script>
