<template>
	<FaDialog
		ref="faDialogRef"
		:width="state.dialogState !== 'add' ? 1200 : 600"
		:full-height="state.dialogState !== 'add'"
		:title="state.dialogTitle"
		:show-fullscreen="state.dialogState !== 'add'"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<div :class="{ 'fa__display_tb-b': state.dialogState !== 'add' }">
			<FaForm
				ref="faFormRef"
				:model="state.formData"
				:rules="state.formRules"
				:disabled="state.formDisabled"
				:cols="state.dialogState !== 'add' ? 2 : 1"
				label-width="100px"
			>
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
				<FaFormItem v-if="state.dialogState !== 'add'" prop="status" label="状态" span="2">
					<el-radio-group v-model="state.formData.status">
						<el-radio :value="1">正常</el-radio>
						<el-radio :value="2">禁用</el-radio>
					</el-radio-group>
				</FaFormItem>
				<FaFormItem prop="remark" label="备注">
					<el-input v-model="state.formData.remark" type="textarea" maxlength="200" placeholder="请输入备注" />
				</FaFormItem>
			</FaForm>
			<ItemTable v-if="state.dialogState !== 'add'" v-model="state.formData.dictionaryItemList" :disabled="state.formDisabled" />
		</div>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { DictionaryValueTypeEnum } from "@/api/enums/DictionaryValueTypeEnum";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import ItemTable from "./components/itemTable.vue";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddDictionaryInput } from "@/api/services/Center/dictionary/models/AddDictionaryInput";
import type { EditDictionaryInput } from "@/api/services/Center/dictionary/models/EditDictionaryInput";

defineOptions({
	name: "DevDictionaryEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditDictionaryInput & AddDictionaryInput>({}),
	formRules: withDefineType<FormRules<EditDictionaryInput & AddDictionaryInput>>({
		dictionaryKey: [{ required: true, message: "请输入字典Key", trigger: "blur" }],
		dictionaryName: [{ required: true, message: "请输入字典名称", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "数据字典",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await dictionaryApi.addDictionary(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await dictionaryApi.editDictionary(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (dictionaryId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await dictionaryApi.queryDictionaryDetail(dictionaryId);
		state.formData = apiRes;
		state.dialogTitle = `数据字典详情 - ${apiRes.dictionaryName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加数据字典";
		state.formDisabled = false;
		state.formData = {
			valueType: DictionaryValueTypeEnum.String,
			status: 1,
		};
		delete state.formData.dictionaryItemList;
	});
};

const edit = (dictionaryId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await dictionaryApi.queryDictionaryDetail(dictionaryId);
		state.formData = apiRes;
		state.dialogTitle = `编辑数据字典 - ${apiRes.dictionaryName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
