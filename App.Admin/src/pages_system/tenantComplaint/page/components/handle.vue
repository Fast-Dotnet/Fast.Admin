<template>
	<FaPopup
		ref="popupRef"
		title="处理投诉"
		position="bottom"
		closable
		:close-on-click-modal="false"
		show-confirm-button
		show-close-button
		@confirm="handleConfirm"
	>
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="处理描述" prop="handleDescription">
				<wd-textarea
					custom-style="--fa-textarea-height: 75px"
					v-model="state.formData.handleDescription"
					clearable
					:maxlength="200"
					placeholder="请输入处理描述"
				/>
			</wd-form-item>
			<wd-form-item title="备注" prop="remark">
				<wd-textarea
					custom-style="--fa-textarea-height: 75px"
					v-model="state.formData.remark"
					clearable
					:maxlength="200"
					placeholder="请输入备注"
				/>
			</wd-form-item>
		</wd-form>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { complaintApi } from "@/api/services/Center/complaint";
import { useFormSchema, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { HandleComplaintInput } from "@/api/services/Center/complaint/models/HandleComplaintInput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemTenantComplaintHandle",
});

const emit = defineEmits(["ok"]);

const popupRef = ref<FaPopupInstance>();
const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<HandleComplaintInput>({}),
	formSchema: useFormSchema<HandleComplaintInput>({
		handleDescription: { required: true, message: "请输入处理描述" },
	}),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	await popupRef.value.close(async () => {
		await complaintApi.handleComplaint(state.formData);
		useToast.success("处理成功！");
		emit("ok");
	});
});

const open = throttle(async (complaintId: string) => {
	await popupRef.value.open(async () => {
		const apiRes = await complaintApi.queryComplaintDetail(complaintId);
		state.formData = apiRes;
	});
});

defineExpose({
	open,
});
</script>
