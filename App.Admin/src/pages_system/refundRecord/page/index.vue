<template>
	<FaPaging :request-api="refundRecordApi.queryRefundRecordPaged">
		<template #top="{ searchParam, setParam, refresh }">
			<FaSearchInput
				:search-param="searchParam"
				data-search-range="Past1M"
				placeholder="搜索退款记录"
				@update:search-param="setParam"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			/>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.bizOrderNo" @click="editFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="PaymentChannelEnum" :value="item.paymentChannel" />
					</view>
				</template>

				<FaField label="手机" :value="item.mobile" />
				<FaField label="OpenId" :value="item.openId" />
				<FaField label="UnionId" :value="item.unionId" />
				<FaField label="退款商户号" :value="item.merchantNo" />
				<FaField label="订单金额" :value="`￥${(item.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" />
				<FaField label="退款金额" :value="`￥${(item.refundAmount ?? 0).toFixed(2) || 0.0} `" color="var(--wot-danger-main)" />
				<FaField label="已退款">
					<FaTag name="BooleanEnum" :value="item.isRefunded" />
				</FaField>
				<FaField label="退款时间" :value="item.refundTime" />
			</FaCard>
		</template>
	</FaPaging>

	<RefundRecordDetail ref="editFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { refundRecordApi } from "@/api/services/Center/refundRecord";
import RefundRecordDetail from "./detail/index.vue";

definePage({
	name: "SystemRefundRecord",
	layout: "layout",
	authCode: "RefundRecord:Paged",
	style: {
		navigationBarTitleText: "退款记录",
	},
});

const editFormRef = ref<InstanceType<typeof RefundRecordDetail>>();
</script>
