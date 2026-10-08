<template>
	<FaSelectPage
		v-bind="$attrs"
		:request-api="accountApi.accountSelector"
		v-model="modelValue"
		v-model:label="mobile"
		placeholder="请选择账号"
		clearable
		more-detail
		@change="handleChange"
	>
		<template #default="data">
			<div style="display: flex; justify-content: space-between; align-items: center; gap: 8px; width: 100%">
				<FaAvatar :src="data.data?.avatar" thumb size="small" />
				<div style="flex: 1">
					<span>{{ data.label }}</span>
					<span style="display: flex; justify-content: space-between; width: 100%">
						<span style="font-size: var(--el-font-size-extra-small); padding-right: 8px">{{ data.data?.nickName }}</span>
						<span style="font-size: var(--el-font-size-extra-small)">{{ data.data?.email }}</span>
					</span>
				</div>
			</div>
		</template>
	</FaSelectPage>
</template>

<script lang="ts" setup>
import { accountApi } from "@/api/services/Center/account";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "AccountSelectPage",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElSelectorOutput | undefined];
}>();

/** 选中的账号 ID */
const modelValue = defineModel<string>();
/** 选中账号的手机号 */
const mobile = defineModel<string>("mobile");
/** 选中账号的邮箱；清空选择时同步清空 */
const email = defineModel<string>("email");
/** 选中账号的标识；清空选择时同步清空 */
const accountKey = defineModel<string>("accountKey");

const handleChange = (data: ElSelectorOutput | ElSelectorOutput[]) => {
	if (Array.isArray(data)) return;
	if (data) {
		email.value = data.data?.email;
		accountKey.value = data.data?.accountKey;
		emit("change", data);
	} else {
		email.value = undefined;
		accountKey.value = undefined;
		emit("change", undefined);
	}
};
</script>
