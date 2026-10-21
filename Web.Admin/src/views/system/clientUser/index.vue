<template>
	<div>
		<FaTable row-key="userId" :request-api="clientUserApi.queryClientUserPaged" :columns="searchColumns">
			<FaTableColumn prop="avatar" label="头像" fixed="left" width="70" small-width="50" sortable type="image" />
			<FaTableColumn prop="mobile" label="手机" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="openId" label="唯一用户标识" width="200" small-width="180" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="unionId" label="统一用户标识" width="200" small-width="180" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="nickName" label="昵称" width="150" small-width="130" sortable />
			<FaTableColumn prop="userType" label="用户类型" width="120" small-width="100" sortable tag :enum="clientUserTypeEnum" />
			<FaTableColumn prop="sex" label="性别" width="100" small-width="80" sortable tag :enum="genderEnum" />
			<FaTableColumn
				prop="lastLoginTime"
				label="最后登录信息"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryClientUserPagedOutput }">
					<span>地区：{{ row.lastLoginProvince }} - {{ row.lastLoginCity }}</span>
					<br />
					<span>Ip：{{ row.lastLoginIp }}</span>
					<br />
					<span>时间：{{ dayjs(row.lastLoginTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					<el-tag v-if="row.lastLoginTime" type="info" round effect="light" size="small" style="margin-left: 5px">
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
				<template #default="{ row }: { row: QueryClientUserPagedOutput }">
					<span>设备：{{ row.lastLoginDevice }}</span>
					<br />
					<span>操作系统：{{ row.lastLoginOS }}</span>
					<br />
					<span>浏览器：{{ row.lastLoginBrowser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="mobileUpdateTime" label="手机号更新时间" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="createdTime" label="创建信息" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="updatedTime" label="更新时间" width="180" small-width="160" sortable type="dateTime" date-fix />
		</FaTable>
	</div>
</template>

<script lang="ts" setup>
import { dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { clientUserApi } from "@/api/services/Center/clientUser";
import { useApp } from "@/stores";
import type { FaTableColumnCtx } from "fast-element-plus";
import type { QueryClientUserPagedOutput } from "@/api/services/Center/clientUser/models/QueryClientUserPagedOutput";

defineOptions({
	name: "SystemClientUser",
});

const appStore = useApp();
const clientUserTypeEnum = appStore.getDictionary("ClientUserTypeEnum");
const genderEnum = appStore.getDictionary("GenderEnum");
const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "appId",
		label: "应用",
		search: { el: "ApplicationSelect" },
	},
	{
		pureSearch: true,
		prop: "userType",
		label: "用户类型",
		enum: clientUserTypeEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "sex",
		label: "性别",
		enum: genderEnum,
		search: { el: "el-select" },
	},
];
</script>
