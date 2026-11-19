<template>
	<FaPopup ref="faPopupRef" title="应用详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="应用名称" :value="formData.appName" selectable />
			<FaField label="应用编号" :value="formData.appNo" copyable />
			<FaField label="版本" center>
				<FaTag name="EditionEnum" :value="formData.edition" />
			</FaField>
			<FaField label="主题色" :value="formData.themeColor" selectable />
			<FaField label="Logo" center>
				<FaImage :src="formData.logoUrl" width="150rpx" height="150rpx" />
			</FaField>
			<FaField label="租户" :value="formData.tenantName" selectable />
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
