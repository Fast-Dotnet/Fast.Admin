<template>
	<FaPopup ref="faPopupRef" title="退款详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="业务编号" :value="formData.bizOrderNo" copyable />
			<FaField label="手机" :value="formData.mobile" copyable />
			<FaField label="OpenId" :value="formData.openId" copyable />
			<FaField label="UnionId" :value="formData.unionId" copyable />
			<FaField label="退款商户号" :value="formData.merchantNo" copyable />
			<FaField label="应用标识" :value="formData.appKey" copyable />
			<FaField label="支付渠道">
				<FaText name="PaymentChannelEnum" :value="formData.paymentChannel" />
			</FaField>
			<FaField label="订单金额" :value="`￥${(formData.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" selectable />
			<FaField label="退款流水号" :value="formData.refundId" copyable />
			<FaField label="退款金额" :value="`￥${(formData.refundAmount ?? 0).toFixed(2)}`" color="var(--wot-danger-main)" selectable />
			<FaField label="已退款" center>
				<FaTag name="BooleanEnum" :value="formData.isRefunded" />
			</FaField>
			<FaField label="退款时间" :value="formData.refundTime" selectable />
			<FaField label="退款状态" :value="formData.refundStatus" selectable />
			<FaField label="回调地址" :value="formData.notifyUrl" copyable />
			<FaField label="省份" :value="formData.province" selectable />
			<FaField label="城市" :value="formData.city" selectable />
			<FaField label="Ip" :value="formData.ip" copyable />
			<FaField label="创建时间" :value="formData.createdTime" selectable />
			<FaField label="设备" :value="formData.device" selectable />
			<FaField label="操作系统" :value="formData.os" selectable />
			<FaField label="浏览器" :value="formData.browser" selectable />
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

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<RefundRecordModel>({});

const detail = throttle(
	async (data: RefundRecordModel) =>
		await faPopupRef.value.open(() => {
			formData.value = data;
		})
);

defineExpose({
	detail,
});
</script>
