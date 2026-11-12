<template>
	<view class="page">
		<wd-cell-group border>
			<wd-cell title="配置编码" :value="state.formData.configCode" />
			<wd-cell title="配置名称" :value="state.formData.configName" />
			<wd-cell title="配置值">
				<wd-text type="primary" :text="state.formData.configValue"></wd-text>
			</wd-cell>
			<wd-cell title="备注" :value="state.formData.remark" />
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
import { configApi } from "@/api/services/Center/config";
import { useMessageBox } from "@/hooks";
import type { QueryConfigDetailOutput } from "@/api/services/Center/config/models/QueryConfigDetailOutput";

definePage({
	name: "DevConfigDetail",
	authCode: "Config:Detail",
	style: {
		navigationBarTitleText: "配置详情",
		enablePullDownRefresh: true,
	},
});

const state = reactive({
	formData: withDefineType<QueryConfigDetailOutput>({}),
	configId: withDefineType<string>(),
});

const handleLoad = async () => {
	const apiRes = await configApi.queryConfigDetail(state.configId);
	state.formData = apiRes;
	uni.setNavigationBarTitle({ title: `配置详情 - ${apiRes.configName}` });
};

onLoad((options: { configId?: string }) => {
	if (!options?.configId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.configId = options.configId;
	handleLoad();
});

onPullDownRefresh(() => {
	handleLoad().finally(() => uni.stopPullDownRefresh());
});
</script>
