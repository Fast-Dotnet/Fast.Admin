<template>
	<FaPopup ref="popupRef" title="在线用户详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="昵称" :value="formData.nickName" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="状态" :value="formData.isOnline ? '在线' : '离线'" />
			<FaField label="姓名" :value="formData.employeeName" />
			<FaField label="工号" :value="formData.employeeNo" />
			<FaField label="部门" :value="formData.departmentName" />
			<FaField label="应用">
				<FaTag name="AppEnvironmentEnum" :value="formData.deviceType" />
				{{ formData.appName }}
			</FaField>
			<FaField label="设备Id" :value="formData.deviceId" selectable />
			<FaField label="会话Id" :value="formData.sessionId" copyable />
			<FaField label="管理员">
				<wd-tag v-if="formData.isSuperAdmin" type="success">超级管理员</wd-tag>
				<wd-tag v-else-if="formData.isAdmin" type="danger">管理员</wd-tag>
				<wd-tag v-else variant="plain">普通用户</wd-tag>
			</FaField>
			<FaField label="省份" :value="formData.lastLoginProvince" />
			<FaField label="城市" :value="formData.lastLoginCity" />
			<FaField label="Ip" :value="formData.lastLoginIp" />
			<FaField label="设备" :value="formData.lastLoginDevice" />
			<FaField label="操作系统" :value="formData.lastLoginOS" />
			<FaField label="浏览器" :value="formData.lastLoginBrowser" />
			<FaField label="登录时间" :value="formData.lastLoginTime" />
			<FaField label="下线时间" :value="formData.offlineTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemTenantOnlineUserDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<TenantOnlineUserModel>();

const detail = (item: TenantOnlineUserModel) => {
	popupRef.value.open(() => {
		formData.value = item;
	});
};

defineExpose({
	detail,
});
</script>
