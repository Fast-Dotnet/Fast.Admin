<template>
	<FaPopup ref="popupRef" title="接口详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="接口名称" :value="formData.apiName" />
			<FaField label="接口地址" :value="formData.apiUrl" copyable />
			<FaField label="请求方式">
				<FaTag name="HttpRequestMethodEnum" :value="formData.method" />
			</FaField>
			<FaField label="操作方式">
				<FaTag name="HttpRequestActionEnum" :value="formData.action" />
			</FaField>
			<FaField label="模块名称" :value="formData.moduleName" />
			<FaField label="服务名称" :value="formData.serviceName" />
			<FaField label="分组名称" :value="formData.groupName" />
			<FaField label="分组标题" :value="formData.groupTitle" />
			<FaField label="授权检测">
				<FaTag name="BooleanEnum" :value="formData.hasAuth" />
			</FaField>
			<FaField label="权限检测">
				<FaTag name="BooleanEnum" :value="formData.hasPermission" />
			</FaField>
			<FaField label="版本" :value="formData.version" />
			<FaField label="排序" :value="formData.sort" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新时间" :value="formData.updatedTime" />

			<template v-if="formData.tags?.length">
				<wd-divider content-position="left">按钮权限</wd-divider>
				<view style="display: flex; gap: 16rpx; margin-top: 8rpx">
					<wd-tag v-for="(tag, index) in formData.tags" :key="index">{{ tag }}</wd-tag>
				</view>
			</template>

			<wd-divider content-position="left">分组描述</wd-divider>
			<wd-rich-text :content="formData.description" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import type { ApiInfoModel } from "@/api/services/Center/api/models/ApiInfoModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevApiDetail",
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<ApiInfoModel>();

const detail = throttle((data: ApiInfoModel) =>
	popupRef.value.open(() => {
		formData.value = data;
	})
);

defineExpose({
	detail,
});
</script>
