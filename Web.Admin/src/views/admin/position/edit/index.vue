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
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled">
			<FaFormItem prop="positionName" label="职位名称">
				<el-input v-model="state.formData.positionName" maxlength="20" placeholder="请输入职位名称" />
			</FaFormItem>
			<FaFormItem prop="sort" label="排序" tips="从小到大">
				<el-input-number v-model="state.formData.sort" :min="1" :max="9999" placeholder="请输入排序" />
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input type="textarea" v-model="state.formData.remark" :rows="2" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { positionApi } from "@/api/services/Admin/position";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddPositionInput } from "@/api/services/Admin/position/models/AddPositionInput";
import type { EditPositionInput } from "@/api/services/Admin/position/models/EditPositionInput";

defineOptions({
	name: "AdminPositionEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditPositionInput & AddPositionInput>({}),
	formRules: withDefineType<FormRules<EditPositionInput & AddPositionInput>>({
		positionName: [{ required: true, message: "请输入职位名称", trigger: "blur" }],
		sort: [{ required: true, message: "请输入排序", trigger: "blur" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "职位",
});

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await positionApi.addPosition(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await positionApi.editPosition(state.formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (positionId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await positionApi.queryPositionDetail(positionId);
		state.formData = apiRes;
		state.dialogTitle = `职位详情 - ${apiRes.positionName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加职位";
		state.formDisabled = false;
		state.formData = {
			sort: 1,
		};
	});
};

const edit = (positionId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await positionApi.queryPositionDetail(positionId);
		state.formData = apiRes;
		state.dialogTitle = `编辑职位 - ${apiRes.positionName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>
