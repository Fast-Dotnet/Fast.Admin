<template>
	<view class="page page__detail">
		<wd-cell title="商户名称" :value="state.formData.merchantName" />
		<wd-cell title="商户号" :value="state.formData.merchantNo" />
		<wd-cell title="商户密钥" :value="state.formData.merchantSecret ? '已配置' : '未配置'" />
		<wd-cell title="商户类型">
			<FaTag name="PaymentChannelEnum" :value="state.formData.merchantType" />
		</wd-cell>
		<wd-cell title="备注" :value="state.formData.remark" />
		<wd-cell title="创建人" :value="state.formData.createdUserName" />
		<wd-cell title="创建时间" :value="state.formData.createdTime" />
		<wd-cell title="更新人" :value="state.formData.updatedUserName" />
		<wd-cell title="更新时间" :value="state.formData.updatedTime" />

		<wd-cell-group title="公钥信息" border>
			<wd-cell title="公钥序号" :value="state.formData.publicSerialNum" :border="false" />
			<template v-if="state.formData.publicKey">
				<wd-cell title="公钥" is-link @click="state.isPublicKeyExpanded = !state.isPublicKeyExpanded" />
				<view v-if="state.isPublicKeyExpanded" class="merchant-pem__body">
					<text class="merchant-pem__text" selectable user-select>{{ state.formData.publicKey }}</text>
				</view>
			</template>
		</wd-cell-group>

		<wd-cell-group title="证书信息" border>
			<wd-cell title="证书序号" :value="state.formData.certSerialNum" :border="false" />
			<template v-if="state.formData.cert">
				<wd-cell title="证书" is-link @click="state.isCertExpanded = !state.isCertExpanded" />
				<view v-if="state.isCertExpanded" class="merchant-pem__body">
					<text class="merchant-pem__text" selectable user-select>{{ state.formData.cert }}</text>
				</view>
			</template>
			<wd-cell title="证书私钥" :value="state.formData.certPrivateKey ? '已配置' : '未配置'" />
		</wd-cell-group>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive } from "vue";
import { withDefineType } from "@fast-china/utils";
import { merchantApi } from "@/api/services/Center/merchant";
import { useMessageBox } from "@/hooks";
import type { QueryMerchantDetailOutput } from "@/api/services/Center/merchant/models/QueryMerchantDetailOutput";

definePage({
	name: "SystemMerchantDetail",
	authCode: "Merchant:Detail",
	style: {
		navigationBarTitleText: "商户号详情",
		enablePullDownRefresh: true,
	},
});

const state = reactive({
	formData: withDefineType<QueryMerchantDetailOutput>({}),
	merchantId: withDefineType<string>(),
	isPublicKeyExpanded: false,
	isCertExpanded: false,
});

const handleLoad = async () => {
	const apiRes = await merchantApi.queryMerchantDetail(state.merchantId);
	state.formData = apiRes;
	state.isPublicKeyExpanded = false;
	state.isCertExpanded = false;
	uni.setNavigationBarTitle({ title: `商户号详情 - ${apiRes.merchantNo}` });
};

onLoad((options: { merchantId?: string }) => {
	if (!options?.merchantId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.merchantId = options.merchantId;
	handleLoad();
});

onPullDownRefresh(() => {
	handleLoad().finally(() => uni.stopPullDownRefresh());
});
</script>

<style lang="scss" scoped>
.merchant-pem__body {
	padding: 0 var(--wot-padding-loose, 12px) var(--wot-padding-loose, 12px);
	background: var(--wot-filled-oppo);
}

.merchant-pem__text {
	display: block;
	padding: 12px;
	border: 1px solid var(--wot-border-light);
	border-radius: 8px;
	background: var(--wot-filled-bottom);
	font-family: monospace;
	font-size: var(--wot-typography-label-size-main);
	line-height: 1.6;
	white-space: pre-wrap;
	overflow-wrap: anywhere;
}
</style>
