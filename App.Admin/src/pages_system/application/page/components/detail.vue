<template>
	<FaPopup ref="faPopupRef" title="应用详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="应用名称" :value="formData.appName" />
			<FaField label="应用编号" :value="formData.appNo" selectable />
			<FaField label="版本" center>
				<FaTag name="EditionEnum" :value="formData.edition" />
			</FaField>
			<FaField label="主题色" :value="formData.themeColor" />
			<FaField label="Logo" center>
				<FaImage :src="formData.logoUrl" width="150rpx" height="150rpx" />
			</FaField>
			<FaField label="租户" :value="formData.tenantName" />
			<FaField label="备注" :value="formData.remark" />
			<FaField label="创建人" :value="formData.createdUserName" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新人" :value="formData.updatedUserName" />
			<FaField label="更新时间" :value="formData.updatedTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { applicationApi } from "@/api/services/Center/application";
import type { QueryApplicationDetailOutput } from "@/api/services/Center/application/models/QueryApplicationDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemApplicationDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryApplicationDetailOutput>({});

const detail = throttle(
	async (appId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await applicationApi.queryApplicationDetail(appId);
		})
);

defineExpose({
	detail,
});
</script>
