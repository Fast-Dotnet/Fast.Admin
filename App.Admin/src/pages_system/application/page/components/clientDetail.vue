<template>
	<FaPopup ref="faPopupRef" title="应用标识详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="应用名称" :value="formData.appName" selectable />
			<FaField label="应用标识" :value="formData.appKey" copyable />
			<FaField label="应用类型">
				<FaTagFlag name="AppEnvironmentEnum" :value="formData.appType" />
			</FaField>
			<FaField label="应用密钥" :value="formData.appSecret ? '已配置' : '未配置'" />
			<FaField label="环境类型" center>
				<FaTag name="EnvironmentTypeEnum" :value="formData.environmentType" />
			</FaField>
			<FaField label="登录组件" :value="formData.loginComponent" selectable />
			<FaField label="请求超时" :value="`${formData.requestTimeout ?? 0}毫秒`" selectable />
			<FaField label="请求加密" center>
				<FaTag name="BooleanEnum" :value="formData.requestEncipher" />
			</FaField>
			<FaField label="微信商户号" :value="formData.weChatMerchantNo" copyable />
			<FaField label="支付宝商户号" :value="formData.alipayMerchantNo" copyable />
			<FaField label="备注" :value="formData.remark" selectable />
			<FaField label="创建人" :value="formData.createdUserName" selectable />
			<FaField label="创建时间" :value="formData.createdTime" selectable />
			<FaField label="更新人" :value="formData.updatedUserName" selectable />
			<FaField label="更新时间" :value="formData.updatedTime" selectable />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { applicationClientApi } from "@/api/services/Center/applicationClient";
import type { QueryApplicationClientDetailOutput } from "@/api/services/Center/applicationClient/models/QueryApplicationClientDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemApplicationClientDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryApplicationClientDetailOutput>({});

const detail = throttle(
	async (recordId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await applicationClientApi.queryApplicationClientDetail(recordId);
		})
);

defineExpose({
	detail,
});
</script>
