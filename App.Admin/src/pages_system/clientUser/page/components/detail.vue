<template>
	<FaPopup ref="popupRef" title="客户端用户详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="昵称" :value="formData.nickName" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="性别" center>
				<FaTag name="GenderEnum" :value="formData.sex" />
			</FaField>
			<FaField label="OpenId" :value="formData.openId" selectable />
			<FaField label="UnionId" :value="formData.unionId" selectable />
			<FaField label="头像">
				<FaImage :src="formData.avatar" width="150rpx" height="150rpx" />
			</FaField>
			<FaField label="手机号更新" :value="formData.mobileUpdateTime" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新时间" :value="formData.updatedTime" />

			<wd-divider content-position="left">最后登录信息</wd-divider>
			<FaField label="省份" :value="formData.lastLoginProvince" />
			<FaField label="城市" :value="formData.lastLoginCity" />
			<FaField label="Ip" :value="formData.lastLoginIp" />
			<FaField label="设备" :value="formData.lastLoginDevice" />
			<FaField label="操作系统" :value="formData.lastLoginOS" />
			<FaField label="浏览器" :value="formData.lastLoginBrowser" />
			<FaField label="时间" :value="formData.lastLoginTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import type { QueryClientUserPagedOutput } from "@/api/services/Center/clientUser/models/QueryClientUserPagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemClientUserDetail",
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryClientUserPagedOutput>();

const detail = throttle(
	async (item: QueryClientUserPagedOutput) =>
		await popupRef.value.open(() => {
			formData.value = item;
		})
);

defineExpose({
	detail,
});
</script>
