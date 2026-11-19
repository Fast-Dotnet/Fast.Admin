<template>
	<FaPaging ref="faPagingRef" page-key="Employee" :request-api="employeeApi.queryEmployeePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaDepartmentTreeSelect v-model="searchParam.value.departmentId" />
					<FaCellPicker title="状态" name="EmployeeStatusEnum" v-model="searchParam.value.status" radio clearable />
					<FaCellPicker title="性别" name="GenderEnum" v-model="searchParam.value.sex" radio clearable />
				</template>
				<template #footer>
					<FaAuth code="Employee:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.employeeId" code="Employee:Detail" @click="detailFormRef.detail(item.employeeId)">
				<view style="display: flex; align-items: center; gap: 20rpx; margin-bottom: 10rpx">
					<FaAvatar :src="item.idPhoto" size="88rpx" original />
					<view style="flex: 1; display: flex; flex-direction: column">
						<text
							style="
								color: var(--wot-text-main);
								font-size: var(--wot-typography-body-size-large);
								font-weight: var(--wot-font-weight-semibold);
							"
						>
							{{ item.employeeName }}
						</text>
						<text>{{ item.employeeNo }}</text>
					</view>
					<FaTag name="EmployeeStatusEnum" :value="item.status" />
				</view>

				<FaField label="手机" :value="item.mobile" copyable />
				<FaField label="邮箱" :value="item.email" copyable />
				<FaField label="登录手机" :value="item.accountMobile" copyable />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="入职日期" :value="item.entryDate?.slice(0, 10)" />
				<FaField v-if="item.status === EmployeeStatusEnum.Resigned" label="离职日期" :value="item.resignDate?.slice(0, 10)" />

				<template #footer>
					<FaAuth v-if="item.status !== EmployeeStatusEnum.Resigned" code="Employee:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.employeeId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Employee:Status">
						<wd-button size="mini" type="primary" variant="plain" @click="handleChangeStatusClick(item)">状态修改</wd-button>
					</FaAuth>
					<FaAuth
						v-if="item.employeeId !== userInfoStore.employeeId && item.status !== EmployeeStatusEnum.Resigned && item.accountMobile"
						code="Employee:Status"
					>
						<wd-button
							v-if="item.accountStatus === CommonStatusEnum.Enable"
							size="mini"
							type="danger"
							variant="plain"
							@click="handleChangeAccountStatus(item)"
						>
							禁用账号
						</wd-button>
						<wd-button v-else size="mini" type="warning" variant="plain" @click="handleChangeAccountStatus(item)">启用账号</wd-button>
					</FaAuth>
					<FaAuth
						v-if="item.employeeId !== userInfoStore.employeeId && item.status !== EmployeeStatusEnum.Resigned && !item.accountMobile"
						code="Employee:Edit"
					>
						<wd-button size="mini" type="primary" variant="plain" @click="bindAccountFormRef.open(item.employeeId)">
							绑定登录账号
						</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<wd-picker v-model:visible="state.statusPickerVisible" :columns="state.statusPickerColumns" @confirm="handleChangeStatus" />

	<EmployeeDetail ref="detailFormRef" />
	<EmployeeResigned ref="resignedFormRef" @ok="faPagingRef.refresh()" />
	<BindAccount ref="bindAccountFormRef" @ok="faPagingRef.refresh()" />
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { EmployeeStatusEnum } from "@/api/enums/EmployeeStatusEnum";
import { employeeApi } from "@/api/services/Admin/employee";
import { useMessageBox, useToast } from "@/hooks";
import { useUserInfo } from "@/stores";
import BindAccount from "./components/bindAccount.vue";
import EmployeeDetail from "./components/detail.vue";
import EmployeeResigned from "./components/resigned.vue";
import type { PickerOption } from "@wot-ui/ui/components/wd-picker-view/types";
import type { QueryEmployeePagedOutput } from "@/api/services/Admin/employee/models/QueryEmployeePagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "AdminEmployee",
	authCode: "Employee:Paged",
	style: {
		navigationBarTitleText: "职员管理",
	},
});

const router = useRouter();
const userInfoStore = useUserInfo();

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof EmployeeDetail>>();
const resignedFormRef = ref<InstanceType<typeof EmployeeResigned>>();
const bindAccountFormRef = ref<InstanceType<typeof BindAccount>>();

const state = reactive({
	clickItem: withDefineType<QueryEmployeePagedOutput>(),
	statusPickerVisible: false,
	statusPickerColumns: withDefineType<PickerOption[]>([]),
});

const handleEdit = throttle(
	async (mode: IPageStateType, employeeId?: string) =>
		await router.push({
			path: "/pages_admin/employee/edit/index",
			query: {
				mode,
				employeeId,
			},
		})
);

const handleChangeStatusClick = (item: QueryEmployeePagedOutput) => {
	state.statusPickerColumns = [
		{ value: EmployeeStatusEnum.Temporary, label: "临时" },
		{ value: EmployeeStatusEnum.Probation, label: "试用" },
		{ value: EmployeeStatusEnum.Intern, label: "实习生" },
		{ value: EmployeeStatusEnum.Outsourcing, label: "外包" },
		{ value: EmployeeStatusEnum.Secondment, label: "挂职" },
		{ value: EmployeeStatusEnum.Formal, label: "正式" },
	].filter(({ value }) => value !== item.status);
	if (item.status !== EmployeeStatusEnum.Resigned && item.employeeId !== userInfoStore.employeeId) {
		state.statusPickerColumns.push({ value: EmployeeStatusEnum.Resigned, label: "离职" });
	}
	state.clickItem = item;
	state.statusPickerVisible = true;
};

/** 修改职员在职状态；恢复在职不会同时启用登录账号。 */
const handleChangeStatus = throttle(async ({ value }: { value: EmployeeStatusEnum[] }) => {
	const { employeeId, employeeName, status, rowVersion } = state.clickItem;
	if (value[0] === EmployeeStatusEnum.Resigned) {
		await resignedFormRef.value.open(employeeId);
		return;
	}
	const message =
		status === EmployeeStatusEnum.Resigned
			? `确定恢复职员【${employeeName}】的在职状态？恢复后仍需单独启用登录账号。`
			: `确定修改职员【${employeeName}】的状态？`;
	await useMessageBox.confirm(message, {
		async beforeConfirm() {
			await employeeApi.changeStatus({
				employeeId,
				status: value[0],
				rowVersion,
			});
			useToast.success("操作成功！");
			faPagingRef.value.refresh();
		},
	});
});

/** 单独切换职员登录账号的启用状态。 */
const handleChangeAccountStatus = throttle(async (item: QueryEmployeePagedOutput) => {
	const { employeeId, accountStatus, rowVersion } = item;
	await useMessageBox.confirm(`确定${accountStatus === CommonStatusEnum.Enable ? "禁用" : "启用"}职员【${item.employeeName}】的登录账号？`, {
		async beforeConfirm() {
			await employeeApi.changeLoginStatus({ employeeId, accountStatus, rowVersion });
			useToast.success("操作成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>
