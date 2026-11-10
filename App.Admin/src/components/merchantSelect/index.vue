<template>
	<FaCellPicker :title="props.title" :value="merchantNo" :is-link="!props.disabled" placeholder="请选择商户号">
		<template #default="{ visible }">
			<FaSelectPage
				title="商户号选择"
				v-model="modelValue"
				v-model:label="merchantNo"
				v-model:visible="visible.value"
				:request-api="() => merchantApi.merchantSelector(props.merchantType)"
				clearable
				:pagination="false"
				disabled-search
				@change="(data) => emit('change', data)"
			>
				<template #default="{ label, data }">
					<view style="display: flex; flex-direction: column; gap: 4rpx">
						<text style="color: var(--wot-text-main); font-weight: var(--wot-font-weight-semibold)">
							{{ data?.merchantName }}
						</text>
						<view style="display: flex; gap: 12rpx">
							<text style="color: var(--wot-text-secondary); font-size: var(--wot-typography-label-size-main)">
								{{ label }}
							</text>
							<FaTag name="PaymentChannelEnum" :value="data?.merchantType" size="small" />
						</view>
					</view>
				</template>
			</FaSelectPage>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { merchantApi } from "@/api/services/Center/merchant";
import type { PropType } from "vue";
import type { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";

defineOptions({
	name: "MerchantSelect",
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
		default: "商户号",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
	/** 用于筛选商户的支付渠道 */
	merchantType: Number as PropType<PaymentChannelEnum>,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的商户号 ID */
const modelValue = defineModel<string>();
/** 选中的商户号 */
const merchantNo = defineModel<string>("merchantNo");
</script>
