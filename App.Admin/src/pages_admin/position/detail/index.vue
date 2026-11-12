<template>
	<view class="page">
		<wd-cell-group border>
			<wd-cell title="职位名称" :value="state.formData.positionName" />
			<wd-cell title="排序" :value="state.formData.sort" />
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
import { positionApi } from "@/api/services/Admin/position";
import { useMessageBox } from "@/hooks";
import type { QueryPositionDetailOutput } from "@/api/services/Admin/position/models/QueryPositionDetailOutput";

definePage({
	name: "AdminPositionDetail",
	authCode: "Position:Detail",
	style: {
		navigationBarTitleText: "职位详情",
		enablePullDownRefresh: true,
	},
});

const state = reactive({
	formData: withDefineType<QueryPositionDetailOutput>({}),
	positionId: withDefineType<string>(),
});

const handleLoad = async () => {
	const apiRes = await positionApi.queryPositionDetail(state.positionId);
	state.formData = apiRes;
	uni.setNavigationBarTitle({ title: `职位详情 - ${apiRes.positionName}` });
};

onLoad((options: { positionId?: string }) => {
	if (!options?.positionId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.positionId = options.positionId;
	handleLoad();
});

onPullDownRefresh(() => {
	handleLoad().finally(() => uni.stopPullDownRefresh());
});
</script>
