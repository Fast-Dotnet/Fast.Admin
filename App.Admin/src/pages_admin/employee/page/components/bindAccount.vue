<template>
	<FaPopup
		ref="popupRef"
		:title="state.popupTitle"
		height="60%"
		position="bottom"
		closable
		:close-on-click-modal="false"
		show-confirm-button
		show-close-button
		confirm-button-text="保存"
		@confirm="handleConfirm"
	>
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="手机" prop="mobile">
				<wd-input v-model="state.formData.mobile" clearable :maxlength="11" inputmode="tel" placeholder="请输入手机" />
			</wd-form-item>
			<wd-form-item title="邮箱" prop="email">
				<wd-input v-model="state.formData.email" clearable :maxlength="50" inputmode="email" placeholder="请输入邮箱" />
			</wd-form-item>
		</wd-form>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { employeeApi } from "@/api/services/Admin/employee";
import { RegExps } from "@/constants";
import { useFormSchema, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { BindLoginAccountInput } from "@/api/services/Admin/employee/models/BindLoginAccountInput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminEmployeeBindAccount",
});

const emit = defineEmits(["ok"]);

const popupRef = ref<FaPopupInstance>();
const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<BindLoginAccountInput>({}),
	formSchema: useFormSchema<BindLoginAccountInput>({
		mobile: [
			{ required: true, message: "请输入手机" },
			{ pattern: RegExps.Mobile, message: "请输入正确的手机号" },
		],
		email: [
			{ required: true, message: "请输入邮箱" },
			{ pattern: RegExps.Email, message: "请输入正确的邮箱" },
		],
	}),
	popupTitle: "绑定登录账号",
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	await popupRef.value.close(async () => {
		await employeeApi.bindLoginAccount(state.formData);
		useToast.success("绑定成功！");
		emit("ok");
	});
});

const open = throttle(async (employeeId: string) => {
	await popupRef.value.open(async () => {
		const apiRes = await employeeApi.queryEmployeeDetail(employeeId);
		state.formData = {
			employeeId: apiRes.employeeId,
			mobile: apiRes.mobile,
			email: apiRes.email,
			rowVersion: apiRes.rowVersion,
		};
		state.popupTitle = `绑定登录账号 - ${apiRes.employeeName}`;
	});
});

defineExpose({
	open,
});
</script>
