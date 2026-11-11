<template>
	<FaPopup ref="popupRef" title="支付详情" height="70%" position="bottom" closable>
		<scroll-view scroll-y>
			<view style="display: flex; flex-direction: column; gap: 10px">
				<view class="log__title">{{ state.formData.bizOrderNo }}</view>
				<FaField label="手机" :value="state.formData.mobile" />
				<FaField label="OpenId" :value="state.formData.openId" />
				<FaField label="UnionId" :value="state.formData.unionId" />
				<FaField label="收款商户号" :value="state.formData.merchantNo" />
				<FaField label="应用标识" :value="state.formData.appKey" />
				<FaField label="支付渠道">
					<FaTag name="PaymentChannelEnum" :value="state.formData.paymentChannel" />
				</FaField>
				<FaField label="订单金额" :value="`￥${(state.formData.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" />
				<FaField label="交易流水号" :value="state.formData.transactionId" />
				<FaField label="支付金额" :value="`￥${(state.formData.paymentAmount ?? 0).toFixed(2) || 0.0} `" color="var(--wot-warning-main)" />
				<FaField label="已支付">
					<FaTag name="BooleanEnum" :value="state.formData.isPaid" />
				</FaField>
				<FaField label="支付时间" :value="state.formData.paymentTime" />
				<template v-if="!state.formData.isPaid">
					<FaField label="关闭状态">
						<FaTag name="BooleanEnum" :value="state.formData.isClosed" />
					</FaField>
					<FaField label="关闭时间" :value="state.formData.closeTime" />
				</template>
				<FaField label="回调地址" :value="state.formData.notifyUrl" />
				<FaField label="地区" :value="`${state.formData.province} - ${state.formData.city}`" />
				<FaField label="Ip" :value="state.formData.ip" />
				<FaField label="创建时间" :value="state.formData.createdTime" />
				<FaField label="设备" :value="state.formData.device" />
				<FaField label="操作系统" :value="state.formData.os" />
				<FaField label="浏览器" :value="state.formData.browser" />
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
import type { PayRecordModel } from "@/api/services/Center/payRecord/models/PayRecordModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemPayRecordDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<PayRecordModel>({}),
});

const detail = (data: PayRecordModel) => {
	popupRef.value?.open(() => {
		state.formData = data;
	});
};

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log__title {
	font-size: var(--wot-typography-body-size-large);
	font-weight: var(--wot-font-weight-semibold);
	overflow-wrap: anywhere;
}
</style>
