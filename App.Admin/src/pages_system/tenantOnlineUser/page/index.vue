<template>
	<FaPaging ref="faPagingRef" page-key="TenantOnlineUser" :request-api="tenantOnlineUserApi.queryTenantOnlineUserPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="设备类型" v-model="searchParam.value.deviceType" name="AppEnvironmentEnum" radio clearable />
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaEmployeeSelect v-model="searchParam.value.employeeId" />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.connectionId" @click="detailFormRef?.detail(item)">
				<view style="display: flex; align-items: center; gap: 20rpx; margin-bottom: 10rpx">
					<FaAvatar :src="item.avatar" size="88rpx" />
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

				<FaField label="应用">
					<FaTag name="AppEnvironmentEnum" :value="item.deviceType" />
					{{ item.appName }}
				</FaField>
				<FaField label="职员" :value="[item.employeeName, item.employeeNo].filter(Boolean).join(' · ')" />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="管理员" center>
					<wd-tag v-if="item.isSuperAdmin" type="success">超级管理员</wd-tag>
					<wd-tag v-else-if="item.isAdmin" type="danger">管理员</wd-tag>
					<wd-tag v-else variant="plain">普通用户</wd-tag>
				</FaField>
				<FaField label="地区" :value="[item.lastLoginProvince, item.lastLoginCity].filter(Boolean).join(' - ')" />
				<FaField label="Ip" :value="item.lastLoginIp" />
				<FaField label="设备" :value="[item.lastLoginDevice, item.lastLoginOS].filter(Boolean).join(' · ')" />
				<FaField label="浏览器" :value="item.lastLoginBrowser" />
				<FaField label="登录时间" :value="item.lastLoginTime" />
				<FaField label="下线时间" :value="item.offlineTime" />

				<template v-if="item.isOnline" #footer>
					<FaAuth code="TenantOnlineUser:ForceOffline">
						<wd-button size="mini" type="warning" variant="plain" @click="handleForceOffline(item)">强制下线</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<TenantOnlineUserDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { tenantOnlineUserApi } from "@/api/services/Center/tenantOnlineUser";
import { useMessageBox, useToast } from "@/hooks";
import TenantOnlineUserDetail from "./components/detail.vue";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "SystemTenantOnlineUser",
	authCode: "TenantOnlineUser:Paged",
	style: {
		navigationBarTitleText: "在线用户",
	},
});

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof TenantOnlineUserDetail>>();

const handleForceOffline = throttle(async (item: TenantOnlineUserModel) => {
	const { connectionId, mobile } = item;
	await useMessageBox.confirm(`确定踢掉账号：【${mobile}】`, {
		async beforeConfirm() {
			await tenantOnlineUserApi.forceOffline({ connectionId });
			useToast.success("强制下线成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>
