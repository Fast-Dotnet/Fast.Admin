<template>
	<div>
		<FaTable ref="faTableRef" row-key="accountId" :request-api="accountApi.queryAccountPaged" hide-search-time>
			<FaTableColumn prop="avatar" label="头像" fixed="left" width="70" small-width="50" sortable type="image" />
			<FaTableColumn
				prop="mobile"
				label="昵称/手机/邮箱"
				fixed="left"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryAccountPagedOutput }">
					<el-button link type="primary" @click="editFormRef.detail(row.accountId)">{{ row.nickName }}</el-button>
					<br />
					手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
					<br />
					邮箱：<span v-iconCopy="row.email">{{ row.email }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="status" label="状态" width="100" small-width="80" sortable tag :enum="appStore.getDictionary('CommonStatusEnum')" />
			<FaTableColumn prop="firstLoginTenantName" label="初次登录租户" width="200" small-width="180" sortable />
			<FaTableColumn
				prop="firstLoginTime"
				label="初次登录信息"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryAccountPagedOutput }">
					<span>地区：{{ row.firstLoginProvince }} - {{ row.firstLoginCity }}</span>
					<br />
					<span>Ip：{{ row.firstLoginIp }}</span>
					<br />
					<span>时间：{{ dayjs(row.firstLoginTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					<el-tag v-if="row.firstLoginTime" type="info" round effect="light" size="small" style="margin-left: 5px">
						{{ formatChineseRelativeTime(row.firstLoginTime) }}
					</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="firstLoginOS"
				label="初次登录设备"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryAccountPagedOutput }">
					<span>设备：{{ row.firstLoginDevice }}</span>
					<br />
					<span>操作系统：{{ row.firstLoginOS }}</span>
					<br />
					<span>浏览器：{{ row.firstLoginBrowser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="lastLoginTenantName" label="最后登录租户" width="200" small-width="180" sortable />
			<FaTableColumn
				prop="lastLoginTime"
				label="最后登录信息"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryAccountPagedOutput }">
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
				<template #default="{ row }: { row: QueryAccountPagedOutput }">
					<span>设备：{{ row.lastLoginDevice }}</span>
					<br />
					<span>操作系统：{{ row.lastLoginOS }}</span>
					<br />
					<span>浏览器：{{ row.lastLoginBrowser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="lockStartTime"
				label="锁定信息"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryAccountPagedOutput }">
					<template v-if="row.isLock">
						<el-tag type="warning">已锁定</el-tag>
						<br />
						<span>错误次数：{{ row.passwordErrorTime }}次</span>
						<br />
						<span>开始时间：{{ dayjs(row.lockStartTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
						<br />
						<span>结束时间：{{ dayjs(row.lockEndTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					</template>
					<template v-else>
						<el-tag type="info">未锁定</el-tag>
					</template>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="createdTime" label="创建时间" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="updatedTime" label="更新时间" width="180" small-width="160" sortable type="dateTime" date-fix />

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryAccountPagedOutput }">
				<el-button v-auth="'Account:Detail'" size="small" plain @click="editFormRef.detail(row.accountId)">详情</el-button>
				<el-button v-auth="'Account:ResetPassword'" size="small" plain type="warning" @click="handleResetPassword(row)">重置密码</el-button>
				<el-button v-if="row.isLock" v-auth="'Account:Unlock'" size="small" plain type="warning" @click="handleUnlock(row)">解锁</el-button>
				<el-button
					v-if="row.status == CommonStatusEnum.Enable"
					v-auth="'Account:Status'"
					size="small"
					plain
					type="danger"
					@click="handleChangeStatus(row)"
				>
					禁用
				</el-button>
				<el-button v-else v-auth="'Account:Status'" size="small" plain type="warning" @click="handleChangeStatus(row)">启用</el-button>
			</template>
		</FaTable>
		<AccountEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { ElMessage, ElMessageBox, dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { accountApi } from "@/api/services/Center/account";
import { useApp } from "@/stores";
import AccountEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QueryAccountPagedOutput } from "@/api/services/Center/account/models/QueryAccountPagedOutput";

defineOptions({
	name: "SystemAccount",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof AccountEdit>>("editFormRef");

/** 处理重置密码 */
const handleResetPassword = (row: QueryAccountPagedOutput) => {
	const { accountId, rowVersion } = row;
	ElMessageBox.confirm("确定重置密码？", { type: "warning" }).then(async () => {
		await accountApi.resetPassword({
			accountId,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await faTableRef.value?.refresh();
	});
};

/** 处理解锁 */
const handleUnlock = (row: QueryAccountPagedOutput) => {
	const { accountId, rowVersion } = row;
	ElMessageBox.confirm("确定解锁账号？", { type: "warning" }).then(async () => {
		await accountApi.unlock({
			accountId,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await faTableRef.value?.refresh();
	});
};

/** 处理状态变更 */
const handleChangeStatus = (row: QueryAccountPagedOutput) => {
	const { accountId, status, rowVersion } = row;
	ElMessageBox.confirm(`确定${status === CommonStatusEnum.Enable ? "禁用" : "启用"}账号？`, { type: "warning" }).then(async () => {
		await accountApi.changeStatus({
			accountId,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await faTableRef.value?.refresh();
	});
};
</script>
