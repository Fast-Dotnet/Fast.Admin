<template>
	<div>
		<FaTable ref="faTableRef" row-key="employeeId" :request-api="employeeApi.queryEmployeePaged" :columns="searchColumns" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Employee:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="idPhoto" label="照片" fixed="left" width="70" small-width="50" sortable type="image" />
			<FaTableColumn
				prop="employeeName"
				label="姓名"
				fixed="left"
				width="300"
				small-width="280"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryEmployeePagedOutput }">
					<el-button link type="primary" @click="editFormRef.detail(row.employeeId)">{{ row.employeeName }}</el-button>
					<br />
					工号：<span v-iconCopy="row.employeeNo">{{ row.employeeNo }}</span>
					<br />
					手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
					<br />
					邮箱：<span v-iconCopy="row.email">{{ row.email }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="mobile" label="账号" width="320" small-width="300" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: QueryEmployeePagedOutput }">
					<template v-if="row.accountMobile">
						<span>昵称：{{ row.accountNickName }}</span>
						<br />
						手机：<span v-iconCopy="row.accountMobile">{{ row.accountMobile }}</span>
						<br />
						邮箱：<span v-iconCopy="row.accountEmail">{{ row.accountEmail }}</span>
						<br />
						<span>
							登陆时间：{{ row.lastLoginTime ?? "" }}
							<el-tag v-if="row.lastLoginTime" type="info" round effect="light" style="margin-left: 5px">
								{{ formatChineseRelativeTime(row.lastLoginTime) }}
							</el-tag>
						</span>
					</template>
					<template v-else>
						<el-text type="info">未绑定登录账号</el-text>
					</template>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="departmentName"
				label="机构/部门"
				width="300"
				small-width="280"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryEmployeePagedOutput }">
					机构：<span>{{ row.orgName }}</span>
					<br />
					<span>{{ row.departmentNames?.join(" > ") ?? "-" }}</span>
					<template v-if="row.isPrincipal">
						<br />
						<el-tag type="primary">负责人</el-tag>
					</template>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="positionName"
				label="职位/职级"
				width="200"
				small-width="180"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: QueryEmployeePagedOutput }">
					职位：<span>{{ row.positionName }}</span>
					<br />
					职级：<span>{{ row.jobLevelName }}</span>
					<br />
					角色：<span>{{ row.roleNames }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="sex" label="性别" width="100" small-width="80" sortable tag :enum="genderEnum" />
			<FaTableColumn prop="status" label="状态" width="100" small-width="80" sortable tag :enum="employeeStatusEnum" />
			<FaTableColumn prop="entryDate" label="入职日期" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="resignDate" label="离职日期" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" sortable />
			<FaTableColumn prop="createdTime" label="创建时间" width="240" small-width="220" sortable type="timeInfo" />
			<FaTableColumn
				prop="updatedTime"
				label="更新时间"
				width="240"
				small-width="220"
				sortable
				type="timeInfo"
				:time-info-field="{ userName: 'updatedUserName', time: 'updatedTime' }"
			/>

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryEmployeePagedOutput }">
				<div style="margin-bottom: 5px">
					<el-button v-auth="'Employee:Detail'" size="small" plain @click="editFormRef.detail(row.employeeId)">详情</el-button>
					<el-button
						v-if="row.status !== EmployeeStatusEnum.Resigned"
						v-auth="'Employee:Edit'"
						size="small"
						plain
						type="primary"
						@click="editFormRef.edit(row.employeeId)"
					>
						编辑
					</el-button>
					<el-dropdown v-auth="'Employee:Status'" style="padding-left: 12px" trigger="click">
						<el-button size="small" plain type="primary">
							更多
							<el-icon class="el-icon--right"><ArrowDown /></el-icon>
						</el-button>
						<template #dropdown>
							<el-dropdown-menu>
								<el-dropdown-item
									v-if="row.status !== EmployeeStatusEnum.Temporary"
									@click="handleChangeStatus(row, EmployeeStatusEnum.Temporary)"
								>
									设为临时
								</el-dropdown-item>
								<el-dropdown-item
									v-if="row.status !== EmployeeStatusEnum.Probation"
									@click="handleChangeStatus(row, EmployeeStatusEnum.Probation)"
								>
									设为试用
								</el-dropdown-item>
								<el-dropdown-item
									v-if="row.status !== EmployeeStatusEnum.Intern"
									@click="handleChangeStatus(row, EmployeeStatusEnum.Intern)"
								>
									设为实习生
								</el-dropdown-item>
								<el-dropdown-item
									v-if="row.status !== EmployeeStatusEnum.Outsourcing"
									@click="handleChangeStatus(row, EmployeeStatusEnum.Outsourcing)"
								>
									设为外包
								</el-dropdown-item>
								<el-dropdown-item
									v-if="row.status !== EmployeeStatusEnum.Secondment"
									@click="handleChangeStatus(row, EmployeeStatusEnum.Secondment)"
								>
									设为挂职
								</el-dropdown-item>
								<el-dropdown-item
									v-if="row.status !== EmployeeStatusEnum.Formal"
									@click="handleChangeStatus(row, EmployeeStatusEnum.Formal)"
								>
									设为正式
								</el-dropdown-item>
								<el-dropdown-item
									v-if="row.status !== EmployeeStatusEnum.Resigned && row.employeeId !== userInfoStore.employeeId"
									@click="resignedEditFormRef.open(row.employeeId)"
								>
									职员离职
								</el-dropdown-item>
							</el-dropdown-menu>
						</template>
					</el-dropdown>
				</div>
				<el-tag v-if="row.employeeId === userInfoStore.employeeId" type="info" effect="light">当前账号</el-tag>
				<el-tag v-else-if="row.status === EmployeeStatusEnum.Resigned" type="danger" effect="light">登录已停用</el-tag>
				<template v-else-if="row.accountMobile">
					<el-button
						v-if="row.accountStatus === CommonStatusEnum.Enable"
						v-auth="'Employee:Status'"
						size="small"
						plain
						type="danger"
						@click="handleChangeAccountStatus(row)"
					>
						禁用账号
					</el-button>
					<el-button v-else v-auth="'Employee:Status'" size="small" plain type="warning" @click="handleChangeAccountStatus(row)">
						启用账号
					</el-button>
				</template>
				<template v-else>
					<el-button v-auth="'Employee:Edit'" size="small" plain type="primary" @click="bindAccountFormRef.open(row.employeeId)">
						绑定登录账号
					</el-button>
				</template>
			</template>
		</FaTable>
		<EmployeeEdit ref="editFormRef" @ok="faTableRef.refresh()" />
		<ResignedEdit ref="resignedEditFormRef" @ok="faTableRef.refresh()" />
		<BindAccount ref="bindAccountFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { ArrowDown, Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { EmployeeStatusEnum } from "@/api/enums/EmployeeStatusEnum";
import { employeeApi } from "@/api/services/Admin/employee";
import { useApp, useUserInfo } from "@/stores";
import BindAccount from "./edit/bindAccount.vue";
import EmployeeEdit from "./edit/index.vue";
import ResignedEdit from "./edit/resignedEdit.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryEmployeePagedOutput } from "@/api/services/Admin/employee/models/QueryEmployeePagedOutput";

defineOptions({
	name: "AdminEmployee",
});

const appStore = useApp();
const genderEnum = appStore.getDictionary("GenderEnum");
const employeeStatusEnum = appStore.getDictionary("EmployeeStatusEnum");

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof EmployeeEdit>>("editFormRef");
const resignedEditFormRef = useTemplateRef<InstanceType<typeof ResignedEdit>>("resignedEditFormRef");
const bindAccountFormRef = useTemplateRef<InstanceType<typeof BindAccount>>("bindAccountFormRef");
const userInfoStore = useUserInfo();

/** 修改职员在职状态；恢复在职不会同时启用登录账号。 */
const handleChangeStatus = (row: QueryEmployeePagedOutput, status: EmployeeStatusEnum) => {
	const { employeeId, rowVersion } = row;
	const message =
		row.status === EmployeeStatusEnum.Resigned
			? `确定恢复职员【${row.employeeName}】的在职状态？恢复后仍需单独启用登录账号。`
			: `确定修改职员【${row.employeeName}】的状态？`;
	ElMessageBox.confirm(message, { type: "warning" }).then(async () => {
		await employeeApi.changeStatus({
			employeeId,
			status,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await faTableRef.value?.refresh();
	});
};

/** 单独切换职员登录账号的启用状态。 */
const handleChangeAccountStatus = (row: QueryEmployeePagedOutput) => {
	const { employeeId, accountStatus, rowVersion } = row;
	ElMessageBox.confirm(`确定${accountStatus === CommonStatusEnum.Enable ? "禁用" : "启用"}职员【${row.employeeName}】的登录账号？`, {
		type: "warning",
	}).then(async () => {
		await employeeApi.changeLoginStatus({
			employeeId,
			accountStatus,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await faTableRef.value?.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "departmentId",
		label: "机构/部门",
		search: { el: "DepartmentTreeSelect" },
	},
	{
		pureSearch: true,
		prop: "status",
		label: "状态",
		enum: employeeStatusEnum,
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
