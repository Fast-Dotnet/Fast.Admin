<template>
	<view class="page page__detail">
		<wd-cell title="头像">
			<FaImage :src="state.formData.avatar" width="88rpx" height="88rpx" original round />
		</wd-cell>
		<wd-cell title="手机" :value="state.formData.mobile" />
		<wd-cell title="邮箱" :value="state.formData.email" />
		<wd-cell title="昵称" :value="state.formData.nickName" />
		<wd-cell title="状态">
			<FaTag name="CommonStatusEnum" :value="state.formData.status" />
		</wd-cell>
		<wd-cell title="创建时间" :value="state.formData.createdTime" />
		<wd-cell title="更新时间" :value="state.formData.updatedTime || '-'" />

		<wd-cell-group title="初次登录信息" border>
			<wd-cell title="租户" :value="state.formData.firstLoginTenantName" />
			<wd-cell title="Ip" :value="state.formData.firstLoginIp" />
			<wd-cell title="设备" :value="state.formData.firstLoginDevice" />
			<wd-cell title="操作系统" :value="state.formData.firstLoginOS" />
			<wd-cell title="浏览器" :value="state.formData.firstLoginBrowser" />
			<wd-cell title="省份" :value="state.formData.firstLoginProvince" />
			<wd-cell title="城市" :value="state.formData.firstLoginCity" />
			<wd-cell title="时间" :value="state.formData.firstLoginTime || '-'" />
		</wd-cell-group>

		<wd-cell-group title="最后登录信息" border>
			<wd-cell title="租户" :value="state.formData.lastLoginTenantName" />
			<wd-cell title="Ip" :value="state.formData.lastLoginIp" />
			<wd-cell title="设备" :value="state.formData.lastLoginDevice" />
			<wd-cell title="操作系统" :value="state.formData.lastLoginOS" />
			<wd-cell title="浏览器" :value="state.formData.lastLoginBrowser" />
			<wd-cell title="省份" :value="state.formData.lastLoginProvince" />
			<wd-cell title="城市" :value="state.formData.lastLoginCity" />
			<wd-cell title="时间" :value="state.formData.lastLoginTime || '-'" />
		</wd-cell-group>

		<wd-cell-group title="锁定信息" border>
			<wd-cell title="锁定">
				<wd-tag v-if="state.formData.lockStartTime" type="warning">已锁定</wd-tag>
				<wd-tag v-else>未锁定</wd-tag>
			</wd-cell>
			<template v-if="state.formData.lockStartTime">
				<wd-cell title="错误次数" :value="`${state.formData.passwordErrorTime}次`" />
				<wd-cell title="锁定时间" :value="`${state.formData.lockStartTime} ~ ${state.formData.lockEndTime}`" />
			</template>
		</wd-cell-group>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive } from "vue";
import { withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import { useMessageBox } from "@/hooks";
import type { QueryAccountDetailOutput } from "@/api/services/Center/account/models/QueryAccountDetailOutput";

definePage({
	name: "SystemAccountDetail",
	authCode: "Account:Detail",
	style: {
		navigationBarTitleText: "账号详情",
		enablePullDownRefresh: true,
	},
});

const state = reactive({
	formData: withDefineType<QueryAccountDetailOutput>({}),
	accountId: withDefineType<string>(),
});

const handleLoad = async () => {
	if (!state.accountId) return;
	const apiRes = await accountApi.queryAccountDetail(state.accountId);
	state.formData = apiRes;
	uni.setNavigationBarTitle({ title: `账号详情 - ${apiRes.nickName}` });
};

onLoad((options: { accountId?: string }) => {
	if (!options?.accountId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.accountId = options.accountId;
	handleLoad();
});

onPullDownRefresh(() => {
	handleLoad().finally(() => uni.stopPullDownRefresh());
});
</script>
