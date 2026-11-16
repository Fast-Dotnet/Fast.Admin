<template>
	<FaPopup ref="popupRef" title="退款详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="业务编号" :value="formData.bizOrderNo" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="OpenId" :value="formData.openId" />
			<FaField label="UnionId" :value="formData.unionId" />
			<FaField label="退款商户号" :value="formData.merchantNo" />
			<FaField label="应用标识" :value="formData.appKey" />
			<FaField label="支付渠道">
				<FaText name="PaymentChannelEnum" :value="formData.paymentChannel" />
			</FaField>
			<FaField label="订单金额" :value="`￥${(formData.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" />
			<FaField label="退款流水号" :value="formData.refundId" copyable />
			<FaField label="退款金额" :value="`￥${(formData.refundAmount ?? 0).toFixed(2)}`" color="var(--wot-danger-main)" />
			<FaField label="已退款">
				<FaTag name="BooleanEnum" :value="formData.isRefunded" />
			</FaField>
			<FaField label="退款时间" :value="formData.refundTime" />
			<FaField label="退款状态" :value="formData.refundStatus" />
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
import { throttle } from "@fast-china/utils";
import type { RefundRecordModel } from "@/api/services/Center/refundRecord/models/RefundRecordModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemRefundRecordDetail",
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<RefundRecordModel>();

const detail = throttle(
	async (data: RefundRecordModel) =>
		await popupRef.value.open(() => {
			formData.value = data;
		})
);

defineExpose({
	detail,
});
</script>
