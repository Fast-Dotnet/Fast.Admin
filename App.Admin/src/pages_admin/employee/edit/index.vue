<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-cell-group title="组织架构" border>
				<FaCellPicker title="机构" prop="orgId" :value="state.formData.orgName" placeholder="请选择机构">
					<template #default="{ visible }">
						<FaTreePicker
							title="机构选择"
							v-model="state.formData.orgId"
							v-model:label="state.formData.orgName"
							v-model:visible="visible.value"
							:request-api="organizationApi.organizationSelector"
							clearable
							check-strictly
							disabled-search
							@clear="
								() => {
									state.formData.departmentId = null;
									state.formData.departmentName = null;
								}
							"
						/>
					</template>
				</FaCellPicker>
				<FaDepartmentTreeSelect
					v-model="state.formData.departmentId"
					v-model:department-name="state.formData.departmentName"
					prop="departmentId"
					:org-id="state.formData.orgId"
					:disabled="!state.formData.orgId"
				/>
				<wd-form-item title="部门负责人" prop="isPrincipal">
					<FaRadioGroup name="BooleanEnum" v-model="state.formData.isPrincipal" direction="horizontal" />
				</wd-form-item>
				<FaCellPicker title="职位" prop="positionId" :value="state.formData.positionName" placeholder="请选择职位">
					<template #default="{ visible }">
						<FaSelectPage
							title="职位选择"
							v-model="state.formData.positionId"
							v-model:label="state.formData.positionName"
							v-model:visible="visible.value"
							:request-api="positionApi.positionSelector"
							clearable
							:pagination="false"
							disabled-search
						/>
					</template>
				</FaCellPicker>
				<FaCellPicker title="职级" prop="jobLevelId" :value="state.formData.jobLevelName" placeholder="请选择职级">
					<template #default="{ visible }">
						<FaSelectPage
							title="职级选择"
							v-model="state.formData.jobLevelId"
							v-model:label="state.formData.jobLevelName"
							v-model:visible="visible.value"
							:request-api="jobLevelApi.jobLevelSelector"
							clearable
							:pagination="false"
							disabled-search
						/>
					</template>
				</FaCellPicker>
			</wd-cell-group>

			<wd-cell-group title="基础档案" border>
				<wd-form-item title="职员名称" prop="employeeName">
					<wd-input v-model="state.formData.employeeName" clearable :maxlength="20" placeholder="请输入职员名称" />
				</wd-form-item>
				<wd-form-item title="手机" prop="mobile">
					<wd-input v-model="state.formData.mobile" clearable :maxlength="11" inputmode="tel" placeholder="请输入手机" />
				</wd-form-item>
				<wd-form-item title="邮箱" prop="email">
					<wd-input v-model="state.formData.email" clearable :maxlength="50" inputmode="email" placeholder="请输入邮箱" />
				</wd-form-item>
				<wd-form-item title="性别" prop="sex">
					<FaRadioGroup name="GenderEnum" v-model="state.formData.sex" direction="horizontal" />
				</wd-form-item>
				<wd-form-item
					title="入职日期"
					prop="entryDate"
					is-link
					:value="state.formData.entryDate"
					placeholder="请选择入职日期"
					@click="state.entryDatePickerVisible = true"
				/>
				<wd-datetime-picker
					v-model="state.entryDateTimestamp"
					v-model:visible="state.entryDatePickerVisible"
					type="date"
					:max-date="Date.now()"
					@confirm="({ value }) => (state.formData.entryDate = dayjs(value).format('YYYY-MM-DD'))"
				/>
				<wd-form-item title="证件照" prop="idPhoto">
					<FaUploadImage v-model="state.formData.idPhoto" :upload-api="fileApi.uploadIdPhoto" />
				</wd-form-item>
				<wd-form-item title="备注" prop="remark">
					<wd-textarea
						v-model="state.formData.remark"
						custom-style="--fa-textarea-height: 75px"
						clearable
						:maxlength="200"
						placeholder="请输入备注"
					/>
				</wd-form-item>
			</wd-cell-group>

			<wd-cell-group title="角色信息" border>
				<wd-form-item title="角色" prop="roleList">
					<wd-checkbox-group v-model="state.formData.roleIds" type="square" direction="horizontal" @change="handleRoleChange">
						<wd-checkbox v-for="role in state.roleList" :key="role.value" :name="role.value" :disabled="role.disabled">
							{{ role.label }}
						</wd-checkbox>
					</wd-checkbox-group>
				</wd-form-item>
			</wd-cell-group>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import dayjs from "dayjs";
import { GenderEnum } from "@/api/enums/GenderEnum";
import { employeeApi } from "@/api/services/Admin/employee";
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import { organizationApi } from "@/api/services/Admin/organization";
import { positionApi } from "@/api/services/Admin/position";
import { roleApi } from "@/api/services/Admin/role";
import { fileApi } from "@/api/services/File";
import { RegExps } from "@/constants";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging, useUserInfo } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddEmployeeInput } from "@/api/services/Admin/employee/models/AddEmployeeInput";
import type { EditEmployeeInput } from "@/api/services/Admin/employee/models/EditEmployeeInput";

definePage({
	name: "AdminEmployeeEdit",
	authCode: ["Employee:Add", "Employee:Edit"],
	style: {
		navigationBarTitleText: "职员",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();
const userInfoStore = useUserInfo();

const formRef = ref<FormInstance>();

type IFormData = EditEmployeeInput &
	AddEmployeeInput & {
		roleIds?: string[];
	};

const state = reactive({
	loading: false,
	formData: withDefineType<IFormData>({}),
	formSchema: useFormSchema<IFormData>({
		orgId: { required: true, message: "请选择机构" },
		departmentId: { required: true, message: "请选择部门" },
		positionId: { required: true, message: "请选择职位" },
		jobLevelId: { required: true, message: "请选择职级" },
		employeeName: { required: true, message: "请输入职员名称" },
		mobile: [
			{ required: true, message: "请输入手机" },
			{ pattern: RegExps.Mobile, message: "请输入正确的手机号" },
		],
		email: [
			{ required: true, message: "请输入邮箱" },
			{ pattern: RegExps.Email, message: "请输入正确的邮箱" },
		],
		idPhoto: { required: true, message: "请上传证件照" },
		entryDate: { required: true, message: "请选择入职日期" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	employeeId: withDefineType<string>(),
	roleList: withDefineType<ElSelectorOutput<string>[]>([]),
	entryDateTimestamp: dayjs().startOf("day").valueOf(),
	entryDatePickerVisible: false,
});

const handleRoleChange = ({ value }: { value: string[] }) => {
	state.formData.roleList = value
		.map((m) => {
			const roleInfo = state.roleList.find((f) => f.value === m);
			if (!roleInfo) return null;

			return {
				employeeId: state.formData.employeeId,
				roleId: roleInfo.value,
				roleName: roleInfo.label,
			};
		})
		.filter(Boolean);
};

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		const { formData } = state;
		switch (state.pageMode) {
			case "add":
				await employeeApi.addEmployee(formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await employeeApi.editEmployee({
					...formData,
					orgList: [
						{
							orgId: formData.orgId,
							departmentId: formData.departmentId,
							isPrimary: true,
							positionId: formData.positionId,
							jobLevelId: formData.jobLevelId,
							isPrincipal: formData.isPrincipal ?? false,
						},
					],
				});
				if (formData.employeeId === userInfoStore.employeeId) {
					await userInfoStore.refreshUserInfo();
				}
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Employee");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_admin/employee/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				{
					uni.setNavigationBarTitle({ title: "添加职员" });
					state.entryDateTimestamp = dayjs().startOf("day").valueOf();
					state.formData = {
						isPrincipal: false,
						sex: GenderEnum.Unknown,
						entryDate: dayjs().format("YYYY-MM-DD"),
						roleIds: [],
						roleList: [],
					};
					state.roleList = await roleApi.roleSelector();
				}
				break;
			case "edit":
				{
					const apiRes = await employeeApi.queryEmployeeDetail(state.employeeId);
					state.formData = apiRes;
					state.formData.roleIds = apiRes.roleList.map((m) => m.roleId);
					const primaryOrg = apiRes.orgList?.find((item) => item.isPrimary) ?? apiRes.orgList?.[0];
					if (primaryOrg) {
						state.formData.orgId = primaryOrg.orgId;
						state.formData.orgName = primaryOrg.orgName;
						state.formData.departmentId = primaryOrg.departmentId;
						state.formData.departmentName = primaryOrg.departmentName;
						state.formData.positionId = primaryOrg.positionId;
						state.formData.positionName = primaryOrg.positionName;
						state.formData.jobLevelId = primaryOrg.jobLevelId;
						state.formData.jobLevelName = primaryOrg.jobLevelName;
						state.formData.isPrincipal = primaryOrg.isPrincipal;
					}
					state.entryDateTimestamp = dayjs(apiRes.entryDate).valueOf();
					state.roleList = await roleApi.roleSelector();
					uni.setNavigationBarTitle({ title: `编辑职员 - ${apiRes.employeeName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; employeeId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.employeeId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.employeeId = options.employeeId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
