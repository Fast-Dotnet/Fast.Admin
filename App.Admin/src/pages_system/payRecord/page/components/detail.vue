<template>
	<FaPopup ref="popupRef" title="支付详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="业务编号" :value="formData.bizOrderNo" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="OpenId" :value="formData.openId" />
			<FaField label="UnionId" :value="formData.unionId" />
			<FaField label="收款商户号" :value="formData.merchantNo" />
			<FaField label="应用标识" :value="formData.appKey" />
			<FaField label="支付渠道">
				<FaText name="PaymentChannelEnum" :value="formData.paymentChannel" />
			</FaField>
			<FaField label="订单金额" :value="`￥${(formData.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" />
			<FaField label="交易流水号" :value="formData.transactionId" copyable />
			<FaField label="支付金额" :value="`￥${(formData.paymentAmount ?? 0).toFixed(2)}`" color="var(--wot-warning-main)" />
			<FaField label="已支付">
				<FaTag name="BooleanEnum" :value="formData.isPaid" />
			</FaField>
			<FaField label="支付时间" :value="formData.paymentTime" />
			<template v-if="!formData.isPaid">
				<FaField label="关闭状态">
					<FaTag name="BooleanEnum" :value="formData.isClosed" />
				</FaField>
				<FaField label="关闭时间" :value="formData.closeTime" />
			</template>
			<FaField label="回调地址" :value="formData.notifyUrl" />
			<FaField label="省份" :value="formData.province" />
			<FaField label="城市" :value="formData.city" />
			<FaField label="Ip" :value="formData.ip" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="设备" :value="formData.device" />
			<FaField label="操作系统" :value="formData.os" />
			<FaField label="浏览器" :value="formData.browser" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
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

const formData = shallowRef<PayRecordModel>();

const detail = (data: PayRecordModel) => {
	popupRef.value.open(() => {
		formData.value = data;
	});
};

defineExpose({
	detail,
});
</script>
