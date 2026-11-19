<template>
	<FaPopup ref="faPopupRef" title="支付详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="业务编号" :value="formData.bizOrderNo" copyable />
			<FaField label="手机" :value="formData.mobile" copyable />
			<FaField label="OpenId" :value="formData.openId" copyable />
			<FaField label="UnionId" :value="formData.unionId" copyable />
			<FaField label="收款商户号" :value="formData.merchantNo" copyable />
			<FaField label="应用标识" :value="formData.appKey" copyable />
			<FaField label="支付渠道">
				<FaText name="PaymentChannelEnum" :value="formData.paymentChannel" />
			</FaField>
			<FaField label="订单金额" :value="`￥${(formData.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" selectable />
			<FaField label="交易流水号" :value="formData.transactionId" copyable />
			<FaField label="支付金额" :value="`￥${(formData.paymentAmount ?? 0).toFixed(2)}`" color="var(--wot-warning-main)" selectable />
			<FaField label="已支付" center>
				<FaTag name="BooleanEnum" :value="formData.isPaid" />
			</FaField>
			<FaField label="支付时间" :value="formData.paymentTime" selectable />
			<template v-if="!formData.isPaid">
				<FaField label="关闭状态" center>
					<FaTag name="BooleanEnum" :value="formData.isClosed" />
				</FaField>
				<FaField label="关闭时间" :value="formData.closeTime" selectable />
			</template>
			<FaField label="回调地址" :value="formData.notifyUrl" copyable />
			<FaField label="省份" :value="formData.province" selectable />
			<FaField label="城市" :value="formData.city" selectable />
			<FaField label="Ip" :value="formData.ip" selectable />
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
import type { PayRecordModel } from "@/api/services/Center/payRecord/models/PayRecordModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemPayRecordDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<PayRecordModel>({});

const detail = throttle(
	async (data: PayRecordModel) =>
		await faPopupRef.value.open(() => {
			formData.value = data;
		})
);

defineExpose({
	detail,
});
</script>
