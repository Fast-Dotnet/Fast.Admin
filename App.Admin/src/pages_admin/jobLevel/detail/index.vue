<template>
	<view class="page">
		<wd-cell-group center border>
			<wd-cell title="职级名称" :value="state.formData.jobLevelName" />
			<wd-cell title="职级等级" :value="state.formData.level" />
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
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import { useMessageBox } from "@/hooks";
import type { QueryJobLevelDetailOutput } from "@/api/services/Admin/jobLevel/models/QueryJobLevelDetailOutput";

definePage({
	name: "AdminJobLevelDetail",
	authCode: "JobLevel:Detail",
	style: {
		navigationBarTitleText: "职级详情",
		enablePullDownRefresh: true,
	},
});

const state = reactive({
	formData: withDefineType<QueryJobLevelDetailOutput>({}),
	jobLevelId: withDefineType<string>(),
});

const handleLoad = async () => {
	const apiRes = await jobLevelApi.queryJobLevelDetail(state.jobLevelId);
	state.formData = apiRes;
	uni.setNavigationBarTitle({ title: `职级详情 - ${apiRes.jobLevelName}` });
};

onLoad((options: { jobLevelId?: string }) => {
	if (!options?.jobLevelId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.jobLevelId = options.jobLevelId;
	handleLoad();
});

onPullDownRefresh(() => {
	handleLoad().finally(() => uni.stopPullDownRefresh());
});
</script>
