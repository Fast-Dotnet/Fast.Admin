<template>
	<FaPopup ref="popupRef" title="客户端用户详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="昵称" :value="formData.nickName" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="性别">
				<FaTag name="GenderEnum" :value="formData.sex" />
			</FaField>
			<FaField label="OpenId" :value="formData.openId" selectable />
			<FaField label="UnionId" :value="formData.unionId" selectable />
			<FaField label="登录省份" :value="formData.lastLoginProvince" />
			<FaField label="登录城市" :value="formData.lastLoginCity" />
			<FaField label="Ip" :value="formData.lastLoginIp" />
			<FaField label="设备" :value="formData.lastLoginDevice" />
			<FaField label="操作系统" :value="formData.lastLoginOS" />
			<FaField label="浏览器" :value="formData.lastLoginBrowser" />
			<FaField label="手机号更新" :value="formData.mobileUpdateTime" />
			<FaField label="登录时间" :value="formData.lastLoginTime" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新时间" :value="formData.updatedTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import type { QueryClientUserPagedOutput } from "@/api/services/Center/clientUser/models/QueryClientUserPagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemClientUserDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryClientUserPagedOutput>();

const detail = (item: QueryClientUserPagedOutput) => {
	popupRef.value.open(() => {
		formData.value = item;
	});
};

defineExpose({
	detail,
});
</script>
