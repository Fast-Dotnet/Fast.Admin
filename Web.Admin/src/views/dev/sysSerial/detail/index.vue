<template>
	<FaDialog ref="faDialogRef" width="800" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" detail-form>
			<FaFormItem label="规则类型">
				<Tag name="SysSerialRuleTypeEnum" :value="state.formData.ruleType" />
			</FaFormItem>
			<FaFormItem label="前缀" :value="state.formData.prefix" />
			<FaFormItem label="时间类型">
				<Tag name="SerialDateTypeEnum" :value="state.formData.dateType" />
			</FaFormItem>
			<FaFormItem label="分隔符">
				<Tag name="SerialSpacerEnum" :value="state.formData.spacer" />
			</FaFormItem>
			<FaFormItem label="长度" :value="state.formData.length" />
			<FaFormItem label="最后序号" :value="state.formData.lastSerial" />
			<FaFormItem label="最后编号" :value="state.formData.lastSerialNo" />
			<FaFormItem label="最后生成时间" :value="state.formData.lastTime" />
			<FaFormItem label="创建人" :value="state.formData.createdUserName" />
			<FaFormItem label="创建时间" :value="state.formData.createdTime" />
			<FaFormItem label="更新人" :value="state.formData.updatedUserName" />
			<FaFormItem label="更新时间" :value="state.formData.updatedTime" />
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { sysSerialApi } from "@/api/services/Center/sysSerial";
import type { FaDialogInstance } from "fast-element-plus";
import type { QuerySysSerialRuleDetailOutput } from "@/api/services/Center/sysSerial/models/QuerySysSerialRuleDetailOutput";

defineOptions({
	name: "DevSysSerialDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QuerySysSerialRuleDetailOutput>({}),
	dialogTitle: "序号",
});

const detail = (serialRuleId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await sysSerialApi.querySysSerialRuleDetail(serialRuleId);
		state.formData = apiRes;
		state.dialogTitle = `序号详情`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>
