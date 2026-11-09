<template>
	<FaCellPicker :title="props.title" :value="employeeName" :is-link="!props.disabled" placeholder="请选择职员">
		<template #default="{ visible }">
			<FaSelectPage
				title="职员选择"
				v-model="modelValue"
				v-model:visible="visible.value"
				:request-api="employeeApi.employeeSelector"
				clearable
				@change="handleChange"
			>
				<template #default="{ label, data }">
					<view style="display: flex; align-items: center; gap: 20rpx">
						<FaAvatar :src="data?.idPhoto" size="72rpx" />
						<view style="display: flex; flex-direction: column; gap: 4rpx">
							<text style="color: var(--wot-text-main); font-weight: var(--wot-font-weight-semibold)">{{ label }}</text>
							<text style="color: var(--wot-text-secondary); font-size: var(--wot-typography-label-size-main)">
								{{ data?.employeeNo }}{{ data?.employeeNo && data?.mobile ? " · " : "" }}{{ data?.mobile }}
							</text>
						</view>
					</view>
				</template>
			</FaSelectPage>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { employeeApi } from "@/api/services/Admin/employee";

defineOptions({
	name: "EmployeeSelect",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 标题 */
	title: {
		type: String,
		default: "职员",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的职员 ID */
const modelValue = defineModel<string>();
/** 选中职员的姓名 */
const employeeName = defineModel<string>("employeeName");
/** 选中职员的工号 */
const employeeNo = defineModel<string>("employeeNo");
/** 选中职员的手机号 */
const mobile = defineModel<string>("mobile");

const handleChange = (item: ElSelectorOutput<string>) => {
	employeeName.value = item?.label;
	employeeNo.value = item?.data?.employeeNo;
	mobile.value = item?.data?.mobile;
	emit("change", item);
};
</script>
