<template>
	<FaPaging page-key="RefundRecord" :request-api="refundRecordApi.queryRefundRecordPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput
				data-search-range="Past1M"
				v-model:search-param="searchParam.value"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			/>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.bizOrderNo" @click="detailFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="PaymentChannelEnum" :value="item.paymentChannel" />
					</view>
				</template>

				<FaField label="手机" :value="item.mobile" />
				<FaField label="退款商户号" :value="item.merchantNo" />
				<FaField label="订单金额" :value="`￥${(item.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" />
				<FaField label="退款金额" :value="`￥${(item.refundAmount ?? 0).toFixed(2)}`" color="var(--wot-danger-main)" />
				<FaField label="已退款" center>
					<FaTag name="BooleanEnum" :value="item.isRefunded" />
				</FaField>
				<FaField label="退款状态" :value="item.refundStatus" />
				<FaField label="退款时间" :value="item.refundTime" />
				<FaField label="创建时间" :value="item.createdTime" />
			</FaCard>
		</template>
	</FaPaging>

	<RefundRecordDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { refundRecordApi } from "@/api/services/Center/refundRecord";
import RefundRecordDetail from "./components/detail.vue";

definePage({
	name: "SystemRefundRecord",
	authCode: "RefundRecord:Paged",
	style: {
		navigationBarTitleText: "退款记录",
	},
});

const detailFormRef = ref<InstanceType<typeof RefundRecordDetail>>();
</script>
