<template>
	<FaPaging :request-api="payRecordApi.queryPayRecordPaged">
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
				<FaField label="收款商户号" :value="item.merchantNo" />
				<FaField label="订单金额" :value="`￥${(item.orderAmount ?? 0).toFixed(2)}`" color="var(--wot-text-auxiliary)" />
				<FaField label="支付金额" :value="`￥${(item.paymentAmount ?? 0).toFixed(2)}`" color="var(--wot-warning-main)" />
				<FaField label="已支付" center>
					<FaTag name="BooleanEnum" :value="item.isPaid" />
				</FaField>
				<FaField label="支付时间" :value="item.paymentTime" />
				<FaField label="创建时间" :value="item.createdTime" />
			</FaCard>
		</template>
	</FaPaging>

	<PayRecordDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { payRecordApi } from "@/api/services/Center/payRecord";
import PayRecordDetail from "./components/detail.vue";

definePage({
	name: "SystemPayRecord",
	authCode: "PayRecord:Paged",
	style: {
		navigationBarTitleText: "支付记录",
	},
});

const detailFormRef = ref<InstanceType<typeof PayRecordDetail>>();
</script>
