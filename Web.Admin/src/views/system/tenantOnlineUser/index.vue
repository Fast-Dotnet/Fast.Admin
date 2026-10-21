<template>
	<div>
		<FaTable
			ref="faTableRef"
			row-key="connectionId"
			:request-api="tenantOnlineUserApi.queryTenantOnlineUserPaged"
			:columns="searchColumns"
			hide-search-time
		>
			<FaTableColumn prop="avatar" label="头像" fixed="left" width="70" small-width="50" sortable type="image" />
			<FaTableColumn
				prop="mobile"
				label="昵称/手机"
				fixed="left"
				width="200"
				small-width="180"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: TenantOnlineUserModel }">
					<span>{{ row.nickName }}</span>
					<br />
					手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="employeeNo"
				label="姓名/工号/部门"
				width="200"
				small-width="180"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: TenantOnlineUserModel }">
					<span>{{ row.employeeName }}</span>
					<br />
					工号：<span v-iconCopy="row.employeeNo">{{ row.employeeNo }}</span>
					<br />
					部门：<span>{{ row.departmentName }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="appNo" label="应用信息" width="300" small-width="280" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: TenantOnlineUserModel }">
					<span>{{ row.appName }}</span>
					<br />
					编号：<span v-iconCopy="row.appNo">{{ row.appNo }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="deviceType" label="设备类型" width="120" small-width="100" sortable tag :enum="appEnvironmentEnum" />
			<FaTableColumn
				prop="deviceId"
				label="设备Id/会话Id"
				width="300"
				small-width="280"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: TenantOnlineUserModel }">
					<span>{{ row.deviceId }}</span>
					<br />
					<span>{{ row.sessionId }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="isSuperAdmin" label="超级管理员" width="120" small-width="100" sortable tag :enum="booleanEnum" />
			<FaTableColumn prop="isAdmin" label="管理员" width="100" small-width="80" sortable tag :enum="booleanEnum" />
			<FaTableColumn prop="isOnline" label="在线" width="100" small-width="80" sortable tag :enum="booleanEnum" />
			<FaTableColumn
				prop="lastLoginTime"
				label="最后登录信息"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: TenantOnlineUserModel }">
					<span>地区：{{ row.lastLoginProvince }} - {{ row.lastLoginCity }}</span>
					<br />
					<span>Ip：{{ row.lastLoginIp }}</span>
					<br />
					<span>时间：{{ dayjs(row.lastLoginTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					<el-tag v-if="row.lastLoginTime" type="info" round effect="light" size="small" style="margin-left: 5px !important">
						{{ formatChineseRelativeTime(row.lastLoginTime) }}
					</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="lastLoginOS"
				label="最后登录设备"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: TenantOnlineUserModel }">
					<span>设备：{{ row.lastLoginDevice }}</span>
					<br />
					<span>操作系统：{{ row.lastLoginOS }}</span>
					<br />
					<span>浏览器：{{ row.lastLoginBrowser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="offlineTime" label="下线时间" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="sessionId" label="会话Id" width="300" small-width="280" copy sortable />

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: TenantOnlineUserModel }">
				<el-button v-auth="'TenantOnlineUser:ForceOffline'" size="small" plain type="warning" @click="handleForceOffline(row)">
					强制下线
				</el-button>
			</template>
		</FaTable>
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { ElMessage, ElMessageBox, dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { tenantOnlineUserApi } from "@/api/services/Center/tenantOnlineUser";
import { useApp } from "@/stores";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { TenantOnlineUserModel } from "@/api/services/Center/tenantOnlineUser/models/TenantOnlineUserModel";

defineOptions({
	name: "SystemTenantOnlineUser",
});

const appStore = useApp();
const appEnvironmentEnum = appStore.getDictionary("AppEnvironmentEnum");
const booleanEnum = appStore.getDictionary("BooleanEnum");

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");

/** 处理强制下线 */
const handleForceOffline = (row: TenantOnlineUserModel) => {
	const { connectionId, mobile } = row;
	ElMessageBox.confirm(`确定踢掉账号：【${mobile}】`, { type: "warning" }).then(async () => {
		await tenantOnlineUserApi.forceOffline({
			connectionId,
		});
		ElMessage.success("强制下线成功！");
		await faTableRef.value?.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "deviceType",
		label: "设备类型",
		enum: appEnvironmentEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "accountId",
		label: "昵称/手机",
		search: { el: "AccountSelectPage" },
	},
	{
		pureSearch: true,
		prop: "employeeId",
		label: "姓名/工号/部门",
		search: { el: "EmployeeSelectPage" },
	},
];
</script>
