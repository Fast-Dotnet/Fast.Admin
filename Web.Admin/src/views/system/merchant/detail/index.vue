<template>
	<FaDialog ref="faDialogRef" width="800" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<FaForm :model="state.formData" class="merchant-detail" detail-form cols="2">
			<FaFormItem label="商户名称" :value="state.formData.merchantName" />
			<FaFormItem label="商户号" :value="state.formData.merchantNo" />
			<FaFormItem label="商户密钥" :value="state.formData.merchantSecret ? '已配置' : '未配置'" />
			<FaFormItem label="商户类型">
				<Tag name="PaymentChannelEnum" :value="state.formData.merchantType" />
			</FaFormItem>
			<FaFormItem label="备注" :value="state.formData.remark" span="2" />
			<FaFormItem label="创建人" :value="state.formData.createdUserName" />
			<FaFormItem label="创建时间" :value="state.formData.createdTime" />
			<FaFormItem label="更新人" :value="state.formData.updatedUserName" />
			<FaFormItem label="更新时间" :value="state.formData.updatedTime" />

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">公钥信息</el-divider>
			</FaLayoutGridItem>

			<FaFormItem label="公钥序号" span="2">
				<div class="merchant-pem">
					<el-space wrap>
						<span>{{ state.formData.publicSerialNum || "-" }}</span>
						<el-button
							v-if="state.formData.publicKey"
							link
							type="primary"
							@click="state.expandedPem = state.expandedPem === 'publicKey' ? null : 'publicKey'"
						>
							{{ state.expandedPem === "publicKey" ? "收起公钥" : "查看公钥" }}
						</el-button>
					</el-space>

					<el-scrollbar
						v-if="state.formData.publicKey && state.expandedPem === 'publicKey'"
						:max-height="180"
						class="merchant-pem__preview"
					>
						<pre>{{ state.formData.publicKey }}</pre>
					</el-scrollbar>
				</div>
			</FaFormItem>

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">证书信息</el-divider>
			</FaLayoutGridItem>

			<FaFormItem label="证书序号" span="2">
				<div class="merchant-pem">
					<el-space wrap>
						<span>{{ state.formData.certSerialNum || "-" }}</span>
						<el-button
							v-if="state.formData.cert"
							link
							type="primary"
							@click="state.expandedPem = state.expandedPem === 'cert' ? null : 'cert'"
						>
							{{ state.expandedPem === "cert" ? "收起证书" : "查看证书" }}
						</el-button>
					</el-space>

					<el-scrollbar v-if="state.formData.cert && state.expandedPem === 'cert'" :max-height="180" class="merchant-pem__preview">
						<pre>{{ state.formData.cert }}</pre>
					</el-scrollbar>
				</div>
			</FaFormItem>

			<FaFormItem label="证书私钥" :value="state.formData.certPrivateKey ? '已配置' : '未配置'" span="2" />
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { merchantApi } from "@/api/services/Center/merchant";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryMerchantDetailOutput } from "@/api/services/Center/merchant/models/QueryMerchantDetailOutput";

defineOptions({
	name: "SystemMerchantDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryMerchantDetailOutput>({}),
	dialogTitle: "商户号",
	expandedPem: withDefineType<"publicKey" | "cert" | null>(null),
});

const detail = (merchantId: string) => {
	state.expandedPem = null;

	faDialogRef.value.open(async () => {
		const apiRes = await merchantApi.queryMerchantDetail(merchantId);
		state.formData = apiRes;
		state.dialogTitle = `商户号详情 - ${apiRes.merchantNo}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>

<style scoped lang="scss">
.merchant-pem {
	flex: 1;
	min-width: 0;
}

.merchant-pem__preview {
	margin-top: 6px;
	border: var(--el-border);
	border-radius: var(--el-border-radius-base);
	background: var(--el-fill-color-light);

	pre {
		margin: 0;
		padding: 8px 12px;
		font-family: ui-monospace, Consolas, monospace;
		font-size: 12px;
		line-height: 1.6;
		white-space: pre-wrap;
		overflow-wrap: anywhere;
	}
}
</style>
