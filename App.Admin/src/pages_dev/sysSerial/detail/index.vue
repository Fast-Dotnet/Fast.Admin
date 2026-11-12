<template>
	<view class="page">
		<wd-cell-group border>
			<wd-cell title="规则类型">
				<FaTag name="SysSerialRuleTypeEnum" :value="state.formData.ruleType" />
			</wd-cell>
			<wd-cell title="前缀" :value="state.formData.prefix" />
			<wd-cell title="时间类型">
				<FaTag name="SerialDateTypeEnum" :value="state.formData.dateType" />
			</wd-cell>
			<wd-cell title="分隔符">
				<FaTag name="SerialSpacerEnum" :value="state.formData.spacer" />
			</wd-cell>
			<wd-cell title="长度" :value="state.formData.length" />
			<wd-cell title="最后序号" :value="state.formData.lastSerial" />
			<wd-cell title="最后编号" :value="state.formData.lastSerialNo" />
			<wd-cell title="最后生成时间" :value="state.formData.lastTime" />
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
import { sysSerialApi } from "@/api/services/Center/sysSerial";
import { useMessageBox } from "@/hooks";
import type { QuerySysSerialRuleDetailOutput } from "@/api/services/Center/sysSerial/models/QuerySysSerialRuleDetailOutput";

definePage({
	name: "DevSysSerialDetail",
	authCode: "SysSerial:Detail",
	style: {
		navigationBarTitleText: "序号详情",
		enablePullDownRefresh: true,
	},
});

const state = reactive({
	formData: withDefineType<QuerySysSerialRuleDetailOutput>({}),
	serialRuleId: withDefineType<string>(),
});

const handleLoad = async () => {
	const apiRes = await sysSerialApi.querySysSerialRuleDetail(state.serialRuleId);
	state.formData = apiRes;
};

onLoad((options: { serialRuleId?: string }) => {
	if (!options?.serialRuleId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.serialRuleId = options.serialRuleId;
	handleLoad();
});

onPullDownRefresh(() => {
	handleLoad().finally(() => uni.stopPullDownRefresh());
});
</script>
