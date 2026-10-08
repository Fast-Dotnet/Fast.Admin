<template>
	<div>
		<FastTable ref="fastTableRef" table-key="1D1KVW17SY" row-key="employeeId" :request-api="employeeApi.queryEmployeePaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Employee:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<template #employeeName="{ row }: { row?: QueryEmployeePagedOutput }">
				<el-button link type="primary" @click="editFormRef.detail(row.employeeId)">{{ row.employeeName }}</el-button>
				<br />
				工号：<span v-iconCopy="row.employeeNo">{{ row.employeeNo }}</span>
				<br />
				手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
				<br />
				邮箱：<span v-iconCopy="row.email">{{ row.email }}</span>
			</template>

			<template #account="{ row }: { row?: QueryEmployeePagedOutput }">
				<template v-if="row.accountMobile">
					<span>昵称：{{ row.accountNickName }}</span>
					<br />
					手机：<span v-iconCopy="row.accountMobile">{{ row.accountMobile }}</span>
					<br />
					邮箱：<span v-iconCopy="row.accountEmail">{{ row.accountEmail }}</span>
					<br />
					<span>
						登陆时间：{{ row.lastLoginTime ?? "" }}
						<el-tag v-if="row.lastLoginTime" type="info" round effect="light" class="ml5">
							{{ formatChineseRelativeTime(row.lastLoginTime) }}
						</el-tag>
					</span>
				</template>
				<template v-else>
					<el-text type="info">未绑定登录账号</el-text>
				</template>
			</template>

			<template #departmentName="{ row }: { row?: QueryEmployeePagedOutput }">
				机构：<span>{{ row.orgName }}</span>
				<br />
				<span>{{ row.departmentNames?.join(" > ") ?? "-" }}</span>
				<template v-if="row.isPrincipal">
					<br />
					<el-tag type="primary">负责人</el-tag>
				</template>
			</template>

			<template #positionName="{ row }: { row?: QueryEmployeePagedOutput }">
				职位：<span>{{ row.positionName }}</span>
				<br />
				职级：<span>{{ row.jobLevelName }}</span>
				<br />
				角色：<span>{{ row.roleNames }}</span>
			</template>

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryEmployeePagedOutput }">
				<div class="mb5">
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
					<el-dropdown v-auth="'Employee:Status'" class="pl12" trigger="click">
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
		</FastTable>
		<EmployeeEdit ref="editFormRef" @ok="fastTableRef.refresh()" />
		<ResignedEdit ref="resignedEditFormRef" @ok="fastTableRef.refresh()" />
		<BindAccount ref="bindAccountFormRef" @ok="fastTableRef.refresh()" />
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
import { useUserInfo } from "@/stores";
import BindAccount from "./edit/bindAccount.vue";
import EmployeeEdit from "./edit/index.vue";
import ResignedEdit from "./edit/resignedEdit.vue";
import type { QueryEmployeePagedOutput } from "@/api/services/Admin/employee/models/QueryEmployeePagedOutput";
import type { FastTableInstance } from "@/components";

defineOptions({
	name: "SystemEmployee",
});

const fastTableRef = useTemplateRef<FastTableInstance>("fastTableRef");
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
	ElMessageBox.confirm(message, {
		type: "warning",
	}).then(async () => {
		await employeeApi.changeStatus({
			employeeId,
			status,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await fastTableRef.value?.refresh();
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
		await fastTableRef.value?.refresh();
	});
};
</script>
