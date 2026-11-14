<template>
	<FaPaging :request-api="tenantOnlineUserApi.queryTenantOnlineUserPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="设备类型" v-model="searchParam.value.deviceType" name="AppEnvironmentEnum" radio clearable />
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaEmployeeSelect v-model="searchParam.value.employeeId" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList, refresh }">
			<FaCard v-for="item in pagedList" :key="item.connectionId">
				<view style="display: flex; align-items: center; gap: 20rpx; margin-bottom: 24rpx">
					<FaImage :src="item.avatar" width="88rpx" height="88rpx" original round />
					<view style="flex: 1; display: flex; flex-direction: column">
						<text
							style="
								color: var(--wot-text-main);
								font-size: var(--wot-typography-body-size-large);
								font-weight: var(--wot-font-weight-semibold);
							"
						>
							{{ item.nickName }}
						</text>
						<text>{{ item.mobile }}</text>
					</view>
					<wd-tag :type="item.isOnline ? 'success' : 'default'">{{ item.isOnline ? "在线" : "离线" }}</wd-tag>
				</view>

				<FaField label="姓名" :value="item.employeeName" />
				<FaField label="工号" :value="item.employeeNo" />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="应用">
					<FaTag name="AppEnvironmentEnum" :value="item.deviceType" />
					{{ item.appName }}
				</FaField>
				<FaField label="设备Id" :value="item.deviceId" />
				<FaField label="会话Id" :value="item.sessionId" copyable />
				<FaField label="管理员">
					<wd-tag v-if="item.isSuperAdmin" type="success">超级管理员</wd-tag>
					<wd-tag v-else-if="item.isAdmin" type="danger">管理员</wd-tag>
					<wd-tag v-else variant="plain">普通用户</wd-tag>
				</FaField>
				<FaField label="地区" :value="`${item.lastLoginProvince} - ${item.lastLoginCity}`" />
				<FaField label="Ip" :value="item.lastLoginIp" />
				<FaField label="设备" :value="item.lastLoginDevice" />
				<FaField label="操作系统" :value="item.lastLoginOS" />
				<FaField label="浏览器" :value="item.lastLoginBrowser" />
				<FaField label="登录时间" :value="item.lastLoginTime" />
				<FaField label="下线时间" :value="item.offlineTime" />

				<template v-if="item.isOnline" #footer>
					<FaAuth code="TenantOnlineUser:ForceOffline">
						<wd-button size="mini" type="warning" variant="plain" @click="handleForceOffline(item, refresh)">强制下线</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { tenantOnlineUserApi } from "@/api/services/Center/tenantOnlineUser";
import { useMessageBox, useToast } from "@/hooks";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";

definePage({
	name: "SystemTenantOnlineUser",
	authCode: "TenantOnlineUser:Paged",
	style: {
		navigationBarTitleText: "在线用户",
	},
});

const handleForceOffline = (item: TenantOnlineUserModel, refresh: () => Promise<void>) => {
	const { connectionId, mobile } = item;
	useMessageBox.confirm(`确定踢掉账号：【${mobile}】`, {
		async beforeConfirm() {
			await tenantOnlineUserApi.forceOffline({ connectionId });
			useToast.success("强制下线成功！");
			refresh();
		},
	});
};
</script>
