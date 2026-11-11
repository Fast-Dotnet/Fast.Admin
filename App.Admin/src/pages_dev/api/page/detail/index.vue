<template>
	<FaPopup ref="popupRef" title="接口详情" height="70%" position="bottom" closable>
		<scroll-view scroll-y>
			<view style="display: flex; flex-direction: column; gap: 10px">
				<view class="log__title">{{ state.formData.apiName }}</view>
				<view style="display: flex; gap: 16rpx; margin-top: 8rpx">
					<FaTag name="HttpRequestActionEnum" :value="state.formData.action" />
					<FaTag name="HttpRequestMethodEnum" :value="state.formData.method" />
				</view>
				<FaField label="接口地址" :value="state.formData.apiUrl" />
				<FaField label="模块名称" :value="state.formData.moduleName" />
				<FaField label="服务名称" :value="state.formData.serviceName" />
				<FaField label="分组名称" :value="state.formData.groupName" />
				<FaField label="分组标题" :value="state.formData.groupTitle" />
				<FaField label="授权检测">
					<FaTag name="BooleanEnum" :value="state.formData.hasAuth" />
				</FaField>
				<FaField label="权限检测">
					<FaTag name="BooleanEnum" :value="state.formData.hasPermission" />
				</FaField>
				<FaField label="版本" :value="state.formData.version" />
				<FaField label="排序" :value="state.formData.sort" />
				<FaField label="创建时间" :value="state.formData.createdTime" />
				<FaField label="更新时间" :value="state.formData.updatedTime" />
				<view v-if="state.formData.tags?.length" style="display: flex; gap: 16rpx; margin-top: 8rpx">
					<wd-tag v-for="tag in state.formData.tags" :key="tag">{{ tag }}</wd-tag>
				</view>

				<view class="log__title">分组描述</view>
				<rich-text :nodes="state.formData.description" />
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
import type { ApiInfoModel } from "@/api/services/Center/api/models/ApiInfoModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevApiDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<ApiInfoModel>({}),
});

const detail = (data: ApiInfoModel) => {
	popupRef.value?.open(() => {
		state.formData = data;
	});
};

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log__title {
	font-size: var(--wot-typography-body-size-large);
	font-weight: var(--wot-font-weight-semibold);
	overflow-wrap: anywhere;
}
</style>
