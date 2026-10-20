<template>
	<FaDialog
		ref="faDialogRef"
		width="800"
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="2">
			<FaFormItem prop="label" label="字典项名称">
				<el-input v-model="state.formData.label" maxlength="50" placeholder="请输入字典项名称" />
			</FaFormItem>
			<FaFormItem prop="value" label="字典项值">
				<el-input v-model="state.formData.value" maxlength="50" placeholder="请输入字典项值" />
			</FaFormItem>
			<FaFormItem prop="type" label="标签类型" span="2">
				<el-radio-group v-model="state.formData.type">
					<el-radio :value="1">Primary</el-radio>
					<el-radio :value="2">Success</el-radio>
					<el-radio :value="4">Info</el-radio>
					<el-radio :value="8">Warning</el-radio>
					<el-radio :value="16">Danger</el-radio>
				</el-radio-group>
			</FaFormItem>
			<FaFormItem prop="visible" label="显示">
				<el-radio-group v-model="state.formData.visible">
					<el-radio :value="true">显示</el-radio>
					<el-radio :value="false">隐藏</el-radio>
				</el-radio-group>
			</FaFormItem>
			<FaFormItem prop="order" label="排序" tips="从小到大">
				<el-input-number v-model="state.formData.order" :min="1" :max="9999" placeholder="请输入顺序" />
			</FaFormItem>
			<FaFormItem prop="status" label="状态">
				<el-radio-group v-model="state.formData.status">
					<el-radio :value="1">正常</el-radio>
					<el-radio :value="2">禁用</el-radio>
				</el-radio-group>
			</FaFormItem>
			<FaFormItem prop="tips" label="提示">
				<el-input v-model="state.formData.tips" type="textarea" maxlength="100" placeholder="请输入提示" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { TagTypeEnum } from "@/api/enums/TagTypeEnum";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { EditDictionaryItemInput } from "@/api/services/Center/dictionary/models/EditDictionaryItemInput";

defineOptions({
	name: "DevDictionaryEditItemEdit",
});

// v-model绑定值
const modelValue = defineModel<EditDictionaryItemInput[]>({ default: () => [], required: true });

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditDictionaryItemInput>({}),
	formRules: withDefineType<FormRules<EditDictionaryItemInput>>({
		label: [{ required: true, message: "请输入字典项名称", trigger: "blur" }],
		value: [{ required: true, message: "请输入字典项值", trigger: "blur" }],
		order: [{ required: true, message: "请输入排序", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "数据字典项",
	tableIndex: withDefineType<number>(),
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				modelValue.value.push({ ...state.formData });
				break;
			case "edit":
				modelValue.value[state.tableIndex] = { ...state.formData };
				break;
		}
	});
};

const detail = (row: EditDictionaryItemInput) => {
	faDialogRef.value.open(() => {
		state.formDisabled = true;
		state.formData = { ...row };
		state.dialogTitle = `数据字典项详情 - ${row.label}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加数据字典项";
		state.formDisabled = false;
		state.formData = {
			type: TagTypeEnum.Primary,
			visible: true,
			status: CommonStatusEnum.Enable,
		};
	});
};

const edit = (row: EditDictionaryItemInput, index: number) => {
	faDialogRef.value.open(() => {
		state.dialogState = "edit";
		state.formDisabled = false;
		state.tableIndex = index;
		state.formData = { ...row };
		state.dialogTitle = `编辑数据字典项 - ${row.label}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
