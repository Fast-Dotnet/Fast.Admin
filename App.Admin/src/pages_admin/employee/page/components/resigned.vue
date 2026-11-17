<template>
	<FaPopup
		ref="faPopupRef"
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
			<wd-text
				custom-style="display: block; margin-bottom: 10rpx"
				type="warning"
				text="离职后将禁用该职员的登录资格，并强制下线其全部在线会话。"
			/>
			<wd-form-item
				title="离职日期"
				prop="resignDate"
				is-link
				:value="state.formData.resignDate"
				placeholder="请选择离职日期"
				@click="state.resignDatePickerVisible = true"
			/>
			<wd-datetime-picker
				v-model="state.resignDateTimestamp"
				v-model:visible="state.resignDatePickerVisible"
				type="date"
				:max-date="Date.now()"
				@confirm="({ value }) => (state.formData.resignDate = dayjs(value).format('YYYY-MM-DD'))"
			/>
			<wd-form-item title="离职原因" prop="resignReason">
				<wd-textarea
					v-model="state.formData.resignReason"
					custom-style="--fa-textarea-height: 75px"
					clearable
					:maxlength="200"
					placeholder="请输入离职原因"
				/>
			</wd-form-item>
		</wd-form>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import dayjs from "dayjs";
import { employeeApi } from "@/api/services/Admin/employee";
import { useFormSchema, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { EmployeeResignedInput } from "@/api/services/Admin/employee/models/EmployeeResignedInput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminEmployeeResigned",
});

const emit = defineEmits(["ok"]);

const faPopupRef = ref<FaPopupInstance>();
const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<EmployeeResignedInput>({}),
	formSchema: useFormSchema<EmployeeResignedInput>({
		resignDate: { required: true, message: "请选择离职日期" },
		resignReason: { required: true, message: "请输入离职原因" },
	}),
	popupTitle: "职员离职",
	resignDateTimestamp: dayjs().startOf("day").valueOf(),
	resignDatePickerVisible: false,
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	await faPopupRef.value.close(async () => {
		await employeeApi.employeeResigned(state.formData);
		useToast.success("离职成功！");
		emit("ok");
	});
});

const open = throttle(async (employeeId: string) => {
	await faPopupRef.value.open(async () => {
		const apiRes = await employeeApi.queryEmployeeDetail(employeeId);
		state.resignDateTimestamp = dayjs().startOf("day").valueOf();
		state.formData = {
			employeeId: apiRes.employeeId,
			resignDate: dayjs().format("YYYY-MM-DD"),
			rowVersion: apiRes.rowVersion,
		};
		state.popupTitle = `职员离职 - ${apiRes.employeeName}`;
	});
});

defineExpose({
	open,
});
</script>
