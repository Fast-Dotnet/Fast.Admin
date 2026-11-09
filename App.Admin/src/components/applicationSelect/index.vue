<template>
	<FaCellPicker :title="props.title" :value="appName" :is-link="!props.disabled" placeholder="请选择应用">
		<template #default="{ visible }">
			<FaSelectPage
				title="应用选择"
				v-model="modelValue"
				v-model:label="appName"
				v-model:visible="visible.value"
				:request-api="applicationApi.applicationSelector"
				clearable
				:pagination="false"
				disabled
				@change="(data) => emit('change', data)"
			>
				<template #default="{ label, data }">
					<view style="display: flex; align-items: center; gap: 20rpx">
						<FaAvatar :src="data.logoUrl" size="72rpx" />
						<view style="display: flex; flex-direction: column; gap: 4rpx">
							<text style="color: var(--wot-text-main); font-weight: var(--wot-font-weight-semibold)">{{ label }}</text>
							<view style="display: flex; gap: 12rpx">
								<text style="color: var(--wot-text-secondary); font-size: var(--wot-typography-label-size-main)">
									{{ data.appNo }}
								</text>
								<FaTag name="EditionEnum" :value="data?.edition" size="small" />
							</view>
						</view>
					</view>
				</template>
			</FaSelectPage>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { applicationApi } from "@/api/services/Center/application";

defineOptions({
	name: "ApplicationSelect",
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
		default: "应用",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的应用 ID */
const modelValue = defineModel<string>();
/** 选中应用的名称 */
const appName = defineModel<string>("appName");
</script>
