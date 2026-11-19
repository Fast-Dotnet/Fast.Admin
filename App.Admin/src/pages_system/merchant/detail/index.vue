<template>
	<view class="page page-detail">
		<FaLoading mask :loading="state.loading" />
		<wd-empty v-if="state.loading || state.loadFailed" icon="no-content" tip="暂无内容" />
		<template v-else>
			<wd-cell-group border>
				<wd-cell title="商户名称">
					<text selectable user-select>{{ state.formData.merchantName }}</text>
				</wd-cell>
				<wd-cell title="商户号">
					<text selectable user-select>{{ state.formData.merchantNo }}</text>
					<template #suffix>
						<button
							v-if="state.formData.merchantNo != null && state.formData.merchantNo !== ''"
							class="fa-field__copy"
							hover-class="none"
							@click.stop="handleCopy(state.formData.merchantNo)"
						>
							<wd-icon name="copy" size="14px" />
							<text>复制</text>
						</button>
					</template>
				</wd-cell>
				<wd-cell title="商户密钥" :value="state.formData.merchantSecret ? '已配置' : '未配置'" />
				<wd-cell title="商户类型">
					<FaTag name="PaymentChannelEnum" :value="state.formData.merchantType" />
				</wd-cell>
				<wd-cell title="备注">
					<text selectable user-select>{{ state.formData.remark }}</text>
				</wd-cell>
				<wd-cell title="创建人">
					<text selectable user-select>{{ state.formData.createdUserName }}</text>
				</wd-cell>
				<wd-cell title="创建时间">
					<text selectable user-select>{{ state.formData.createdTime }}</text>
				</wd-cell>
				<wd-cell title="更新人">
					<text selectable user-select>{{ state.formData.updatedUserName }}</text>
				</wd-cell>
				<wd-cell title="更新时间">
					<text selectable user-select>{{ state.formData.updatedTime }}</text>
				</wd-cell>
			</wd-cell-group>

			<wd-cell-group title="公钥信息" border>
				<wd-cell title="公钥序号">
					<text selectable user-select>{{ state.formData.publicSerialNum }}</text>
					<template #suffix>
						<button
							v-if="state.formData.publicSerialNum != null && state.formData.publicSerialNum !== ''"
							class="fa-field__copy"
							hover-class="none"
							@click.stop="handleCopy(state.formData.publicSerialNum)"
						>
							<wd-icon name="copy" size="14px" />
							<text>复制</text>
						</button>
					</template>
				</wd-cell>
				<template v-if="state.formData.publicKey">
					<wd-cell title="公钥" clickable @click="state.isPublicKeyExpanded = !state.isPublicKeyExpanded">
						<template #suffix>
							<button class="fa-field__copy" hover-class="none" @click.stop="handleCopy(state.formData.publicKey)">
								<wd-icon name="copy" size="14px" />
								<text>复制</text>
							</button>
							<wd-icon :name="state.isPublicKeyExpanded ? 'arrow-down' : 'arrow-right'" />
						</template>
					</wd-cell>
					<view v-if="state.isPublicKeyExpanded" class="merchant-pem__body">
						<text class="merchant-pem__text" selectable user-select>{{ state.formData.publicKey }}</text>
					</view>
				</template>
			</wd-cell-group>

			<wd-cell-group title="证书信息" border>
				<wd-cell title="证书序号">
					<text selectable user-select>{{ state.formData.certSerialNum }}</text>
					<template #suffix>
						<button
							v-if="state.formData.certSerialNum != null && state.formData.certSerialNum !== ''"
							class="fa-field__copy"
							hover-class="none"
							@click.stop="handleCopy(state.formData.certSerialNum)"
						>
							<wd-icon name="copy" size="14px" />
							<text>复制</text>
						</button>
					</template>
				</wd-cell>
				<template v-if="state.formData.cert">
					<wd-cell title="证书" clickable @click="state.isCertExpanded = !state.isCertExpanded">
						<template #suffix>
							<button class="fa-field__copy" hover-class="none" @click.stop="handleCopy(state.formData.cert)">
								<wd-icon name="copy" size="14px" />
								<text>复制</text>
							</button>
							<wd-icon :name="state.isCertExpanded ? 'arrow-down' : 'arrow-right'" />
						</template>
					</wd-cell>
					<view v-if="state.isCertExpanded" class="merchant-pem__body">
						<text class="merchant-pem__text" selectable user-select>{{ state.formData.cert }}</text>
					</view>
				</template>
				<wd-cell title="证书私钥" :value="state.formData.certPrivateKey ? '已配置' : '未配置'" />
			</wd-cell-group>
		</template>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive } from "vue";
import { copy, withDefineType } from "@fast-china/utils";
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
	loading: false,
	loadFailed: false,
	formData: withDefineType<QueryMerchantDetailOutput>({}),
	merchantId: withDefineType<string>(),
	isPublicKeyExpanded: false,
	isCertExpanded: false,
});

const handleCopy = async (content: string) => {
	try {
		await copy(content);
	} catch {
		uni.showToast({ title: "复制失败", icon: "none" });
	}
};

const handleLoad = async () => {
	state.loading = true;
	state.loadFailed = false;
	try {
		const apiRes = await merchantApi.queryMerchantDetail(state.merchantId);
		state.formData = apiRes;
		state.isPublicKeyExpanded = false;
		state.isCertExpanded = false;
		uni.setNavigationBarTitle({ title: `商户号详情 - ${apiRes.merchantNo}` });
	} catch {
		state.loadFailed = true;
	} finally {
		state.loading = false;
	}
};

onLoad((options: { merchantId?: string }) => {
	if (!options?.merchantId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.merchantId = options.merchantId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
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
