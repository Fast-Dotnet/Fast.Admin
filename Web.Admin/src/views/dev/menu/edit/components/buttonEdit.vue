<template>
	<FaDialog
		ref="faDialogRef"
		width="500"
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="1">
			<FaFormItem prop="buttonName" label="按钮名称">
				<el-input v-model="state.formData.buttonName" maxlength="50" placeholder="请输入按钮名称" />
			</FaFormItem>
			<FaFormItem prop="buttonCode" label="按钮编码">
				<el-input v-model="state.formData.buttonCode" maxlength="50" placeholder="请输入按钮编码" />
			</FaFormItem>
			<FaFormItem prop="edition" label="版本" span="2">
				<RadioGroup name="EditionEnum" v-model="state.formData.edition" />
			</FaFormItem>
			<FaFormItem prop="roleType" label="角色" span="4">
				<el-checkbox-group v-model="state.formData.roleTypes">
					<el-checkbox v-for="(item, index) in roleTypeEnum" :key="index" :label="item.label" :value="item.value" />
				</el-checkbox-group>
			</FaFormItem>
			<FaFormItem prop="hasWeb" label="Web端">
				<el-checkbox v-model="state.formData.hasWeb">Web端</el-checkbox>
			</FaFormItem>
			<FaFormItem prop="hasMobile" label="移动端">
				<el-checkbox v-model="state.formData.hasMobile">移动端</el-checkbox>
			</FaFormItem>
			<FaFormItem prop="hasDesktop" label="桌面端">
				<el-checkbox v-model="state.formData.hasDesktop">桌面端</el-checkbox>
			</FaFormItem>
			<FaFormItem prop="sort" label="排序" tips="从小到大">
				<el-input-number v-model="state.formData.sort" :min="1" :max="99999" placeholder="请输入排序" />
			</FaFormItem>
			<FaFormItem prop="status" label="状态">
				<RadioGroup button name="CommonStatusEnum" v-model="state.formData.status" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";
import { useApp } from "@/stores";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { EditMenuButtonInput } from "@/api/services/Center/menu/models/EditMenuButtonInput";

defineOptions({
	name: "DevMenuEditButtonEdit",
});

const appStore = useApp();
const roleTypeEnum = appStore.getDictionary("RoleTypeEnum");

// v-model绑定值
const modelValue = defineModel<EditMenuButtonInput[]>({ required: true });

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

type IFormData = EditMenuButtonInput & {
	roleTypes?: RoleTypeEnum[];
};

const state = reactive({
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<IFormData>>({
		buttonCode: [{ required: true, message: "请输入按钮编码", trigger: "blur" }],
		buttonName: [{ required: true, message: "请输入按钮名称", trigger: "blur" }],
		sort: [{ required: true, message: "请输入排序", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "按钮",
	tableIndex: withDefineType<number>(),
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		const { formData, dialogState } = state;
		if (formData.roleTypes?.length > 0) {
			let _roleType = 0;
			formData.roleTypes.forEach((item) => (_roleType |= item));
			formData.roleType = _roleType;
		}

		switch (dialogState) {
			case "add":
				modelValue.value.push({ ...formData });
				break;
			case "edit":
				modelValue.value[state.tableIndex] = { ...formData };
				break;
		}
	});
};

const handleFlagsEnum = () => {
	state.formData.roleTypes = [];
	if (state.formData.roleType) {
		for (const key in RoleTypeEnum) {
			const item = RoleTypeEnum[key];
			if (typeof item !== "number") {
				continue;
			}
			if (item === 0) {
				continue;
			}
			if ((state.formData.roleType & item) !== 0) {
				state.formData.roleTypes.push(item);
			}
		}
	}
};

const detail = (row: EditMenuButtonInput) => {
	faDialogRef.value.open(() => {
		state.formDisabled = true;
		state.formData = { ...row };
		state.dialogTitle = `按钮详情 - ${row.buttonName}`;
		handleFlagsEnum();
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加按钮";
		state.formDisabled = false;
		state.formData = {
			edition: EditionEnum.None,
			hasWeb: true,
			hasMobile: false,
			hasDesktop: false,
			sort: 1,
			status: CommonStatusEnum.Enable,
		};
	});
};

const edit = (row: EditMenuButtonInput, index: number) => {
	faDialogRef.value.open(() => {
		state.dialogState = "edit";
		state.formDisabled = false;
		state.tableIndex = index;
		state.formData = { ...row };
		state.dialogTitle = `编辑按钮 - ${row.buttonName}`;
		handleFlagsEnum();
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
