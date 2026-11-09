<template>
	<FaCellPicker :title="props.title" :value="mobile" :is-link="!props.disabled" placeholder="请选择账号">
		<template #default="{ visible }">
			<FaSelectPage
				title="账号选择"
				v-model="modelValue"
				v-model:visible="visible.value"
				:request-api="accountApi.accountSelector"
				clearable
				@change="handleChange"
			>
				<template #default="{ label, data }">
					<view style="display: flex; align-items: center; gap: 20rpx">
						<FaAvatar :src="data?.avatar" size="72rpx" />
						<view style="display: flex; flex-direction: column; gap: 4rpx">
							<text style="color: var(--wot-text-main); font-weight: var(--wot-font-weight-semibold)">{{ label }}</text>
							<text style="color: var(--wot-text-secondary); font-size: var(--wot-typography-label-size-main)">
								{{ data?.nickName }}{{ data?.nickName && data?.email ? " · " : "" }}{{ data?.email }}
							</text>
						</view>
					</view>
				</template>
			</FaSelectPage>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { accountApi } from "@/api/services/Center/account";

defineOptions({
	name: "AccountSelect",
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
		default: "账号",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的账号 ID */
const modelValue = defineModel<string>();
/** 选中的手机号 */
const mobile = defineModel<string>("mobile");
/** 选中账号的邮箱 */
const email = defineModel<string>("email");
/** 选中账号的标识 */
const accountKey = defineModel<string>("accountKey");

const handleChange = (item: ElSelectorOutput<string>) => {
	mobile.value = item?.label;
	email.value = item?.data?.email;
	accountKey.value = item?.data?.accountKey;
	emit("change", item);
};
</script>
