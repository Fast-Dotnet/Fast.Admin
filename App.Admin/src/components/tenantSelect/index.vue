<template>
	<FaCellPicker :title="props.title" :value="tenantName" :is-link="!props.disabled" placeholder="请选择租户">
		<template #default="{ visible }">
			<FaSelectPage
				title="租户选择"
				v-model="modelValue"
				v-model:visible="visible.value"
				:request-api="tenantApi.tenantSelector"
				clearable
				@change="handleChange"
			>
				<template #default="{ label, data }">
					<view style="display: flex; align-items: center; gap: 20rpx">
						<FaAvatar :src="data.logoUrl" size="72rpx" />
						<view style="display: flex; flex-direction: column; gap: 4rpx">
							<text style="color: var(--wot-text-main); font-weight: var(--wot-font-weight-semibold)">{{ label }}</text>
							<view style="display: flex; gap: 12rpx">
								<text style="color: var(--wot-text-secondary); font-size: var(--wot-typography-label-size-main)">
									{{ data?.tenantNo }}
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
import { tenantApi } from "@/api/services/Center/tenant";

defineOptions({
	name: "TenantSelect",
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
		default: "租户",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的租户 ID */
const modelValue = defineModel<string>();
/** 选中租户的名称 */
const tenantName = defineModel<string>("tenantName");
/** 选中租户的编号 */
const tenantNo = defineModel<string>("tenantNo");
/** 选中租户的编码 */
const tenantCode = defineModel<string>("tenantCode");

const handleChange = (item: ElSelectorOutput<string>) => {
	tenantName.value = item?.label;
	tenantNo.value = item?.data?.tenantNo;
	tenantCode.value = item?.data?.tenantCode;
	emit("change", item);
};
</script>
