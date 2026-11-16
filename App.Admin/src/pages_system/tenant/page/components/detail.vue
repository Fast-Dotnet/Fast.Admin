<template>
	<FaPopup ref="popupRef" title="租户详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="租户名称" :value="formData.tenantName" />
			<FaField label="租户编号" :value="formData.tenantNo" />
			<FaField label="租户编码" :value="formData.tenantCode" />
			<FaField label="租户简称" :value="formData.shortName" />
			<FaField label="租户英文名称" :value="formData.spellName" />
			<FaField label="版本">
				<FaTag name="EditionEnum" :value="formData.edition" />
			</FaField>
			<FaField label="租户类型">
				<FaTag name="TenantTypeEnum" :value="formData.tenantType" />
			</FaField>
			<FaField label="机器人名称" :value="formData.robotName" />
			<FaField label="状态">
				<FaTag name="CommonStatusEnum" :value="formData.status" />
			</FaField>
			<FaField label="Logo">
				<FaImage :src="formData.logoUrl" width="150rpx" height="150rpx" />
			</FaField>

			<wd-divider content-position="left">管理员信息</wd-divider>
			<FaField label="管理员名称" :value="formData.adminName" />
			<FaField label="管理员手机" :value="formData.adminMobile" />
			<FaField label="管理员邮箱" :value="formData.adminEmail" />
			<FaField label="管理员电话" :value="formData.adminPhone" />

			<wd-divider content-position="left">阿里云</wd-divider>
			<FaField label="签名名称" :value="formData.aliyunSignName" />

			<wd-divider content-position="left">其他信息</wd-divider>
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
import { tenantApi } from "@/api/services/Center/tenant";
import type { QueryTenantDetailOutput } from "@/api/services/Center/tenant/models/QueryTenantDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemTenantDetail",
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryTenantDetailOutput>();

const detail = throttle(
	async (tenantId: string) =>
		await popupRef.value.open(async () => {
			formData.value = await tenantApi.queryTenantDetail(tenantId);
		})
);

defineExpose({
	detail,
});
</script>
