<template>
	<view class="page page__detail">
		<wd-cell title="Logo">
			<FaImage :src="state.formData.logoUrl" width="88rpx" height="88rpx" />
		</wd-cell>
		<wd-cell title="租户名称" :value="state.formData.tenantName" />
		<wd-cell title="租户编号" :value="state.formData.tenantNo" />
		<wd-cell title="租户编码" :value="state.formData.tenantCode" />
		<wd-cell title="租户简称" :value="state.formData.shortName" />
		<wd-cell title="租户英文名称" :value="state.formData.spellName" />
		<wd-cell title="版本">
			<FaTag name="EditionEnum" :value="state.formData.edition" />
		</wd-cell>
		<wd-cell title="租户类型">
			<FaTag name="TenantTypeEnum" :value="state.formData.tenantType" />
		</wd-cell>
		<wd-cell title="机器人名称" :value="state.formData.robotName" />
		<wd-cell title="状态">
			<FaTag name="CommonStatusEnum" :value="state.formData.status" />
		</wd-cell>

		<wd-cell-group title="管理员信息" border>
			<wd-cell title="管理员名称" :value="state.formData.adminName" />
			<wd-cell title="管理员手机" :value="state.formData.adminMobile" />
			<wd-cell title="管理员邮箱" :value="state.formData.adminEmail" />
			<wd-cell title="管理员电话" :value="state.formData.adminPhone" />
		</wd-cell-group>

		<wd-cell-group title="阿里云" border>
			<wd-cell title="签名名称" :value="state.formData.aliyunSignName" />
		</wd-cell-group>

		<wd-cell-group title="其他信息" border>
			<wd-cell title="创建人" :value="state.formData.createdUserName" />
			<wd-cell title="创建时间" :value="state.formData.createdTime" />
			<wd-cell title="更新人" :value="state.formData.updatedUserName" />
			<wd-cell title="更新时间" :value="state.formData.updatedTime" />
		</wd-cell-group>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive } from "vue";
import { withDefineType } from "@fast-china/utils";
import { tenantApi } from "@/api/services/Center/tenant";
import { useMessageBox } from "@/hooks";
import type { QueryTenantDetailOutput } from "@/api/services/Center/tenant/models/QueryTenantDetailOutput";

definePage({
	name: "SystemTenantDetail",
	authCode: "Tenant:Detail",
	style: {
		navigationBarTitleText: "租户详情",
		enablePullDownRefresh: true,
	},
});

const state = reactive({
	formData: withDefineType<QueryTenantDetailOutput>({}),
	tenantId: withDefineType<string>(),
});

const handleLoad = async () => {
	const apiRes = await tenantApi.queryTenantDetail(state.tenantId);
	state.formData = apiRes;
	uni.setNavigationBarTitle({ title: `租户详情 - ${apiRes.tenantName}` });
};

onLoad((options: { tenantId?: string }) => {
	if (!options?.tenantId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.tenantId = options.tenantId;
	handleLoad();
});

onPullDownRefresh(() => {
	handleLoad().finally(() => uni.stopPullDownRefresh());
});
</script>
