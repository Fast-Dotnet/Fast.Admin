<template>
	<FaPopup ref="faPopupRef" title="在线用户详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="昵称" :value="formData.nickName" selectable />
			<FaField label="手机" :value="formData.mobile" copyable />
			<FaField label="状态" center>
				<wd-tag v-if="formData.isOnline" type="success">在线</wd-tag>
				<wd-tag v-else>离线</wd-tag>
			</FaField>
			<FaField label="姓名" :value="formData.employeeName" selectable />
			<FaField label="工号" :value="formData.employeeNo" copyable />
			<FaField label="部门" :value="formData.departmentName" selectable />
			<FaField label="应用名称" :value="formData.appName" selectable />
			<FaField label="应用编号" :value="formData.appNo" copyable />
			<FaField label="设备类型" center>
				<FaTag name="AppEnvironmentEnum" :value="formData.deviceType" />
			</FaField>
			<FaField label="设备Id" :value="formData.deviceId" copyable />
			<FaField label="会话Id" :value="formData.sessionId" copyable />
			<FaField label="管理员" center>
				<wd-tag v-if="formData.isSuperAdmin" type="success">超级管理员</wd-tag>
				<wd-tag v-else-if="formData.isAdmin" type="danger">管理员</wd-tag>
				<wd-tag v-else variant="plain">普通用户</wd-tag>
			</FaField>
			<FaField label="省份" :value="formData.lastLoginProvince" selectable />
			<FaField label="城市" :value="formData.lastLoginCity" selectable />
			<FaField label="Ip" :value="formData.lastLoginIp" selectable />
			<FaField label="设备" :value="formData.lastLoginDevice" selectable />
			<FaField label="操作系统" :value="formData.lastLoginOS" selectable />
			<FaField label="浏览器" :value="formData.lastLoginBrowser" selectable />
			<FaField label="登录时间" :value="formData.lastLoginTime" selectable />
			<FaField label="下线时间" :value="formData.offlineTime" selectable />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemTenantOnlineUserDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<TenantOnlineUserModel>({});

const detail = throttle(
	async (item: TenantOnlineUserModel) =>
		await faPopupRef.value.open(() => {
			formData.value = item;
		})
);

defineExpose({
	detail,
});
</script>
