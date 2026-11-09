<template>
	<FaCellPicker :title="props.title" :value="modelValue" :is-link="!props.disabled" placeholder="请选择应用标识">
		<template #default="{ visible }">
			<FaSelectPage
				title="应用标识选择"
				v-model="modelValue"
				v-model:visible="visible.value"
				:request-api="applicationClientApi.applicationClientSelector"
				clearable
				tree-select
				:pagination="false"
				disabled
				@change="handleChange"
			>
				<template #default="{ label, data }">
					<view style="display: flex; flex-direction: column; gap: 4rpx">
						<text style="color: var(--wot-text-main); font-weight: var(--wot-font-weight-semibold)">{{ label }}</text>
						<view style="display: flex; gap: 12rpx">
							<text style="color: var(--wot-text-secondary); font-size: var(--wot-typography-label-size-main)">
								{{ data.appName }}
							</text>
							<FaTagFlag name="AppEnvironmentEnum" :value="data?.appType" size="small" />
						</view>
					</view>
				</template>
			</FaSelectPage>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { applicationClientApi } from "@/api/services/Center/applicationClient";

defineOptions({
	name: "ApplicationClientSelect",
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
		default: "应用标识",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的应用标识 */
const modelValue = defineModel<string>();
/** 选中标识所属的应用 ID */
const appId = defineModel<string>("appId");
/** 选中标识所属的应用名称 */
const appName = defineModel<string>("appName");

const handleChange = (item: ElSelectorOutput<string>) => {
	appId.value = item?.data?.appId;
	appName.value = item?.data?.appName;
	emit("change", item);
};
</script>
