<template>
	<FaDialog
		ref="faDialogRef"
		width="1200"
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="3">
			<FaLayoutGridItem span="3">
				<el-divider content-position="left">组织架构</el-divider>
			</FaLayoutGridItem>
			<FaFormItem prop="orgId" label="机构">
				<FaTreeSelect
					:request-api="organizationApi.organizationSelector"
					v-model="state.formData.orgId"
					v-model:label="state.formData.orgName"
					placeholder="请选择机构"
					check-strictly
					filterable
					clearable
				/>
			</FaFormItem>
			<FaFormItem prop="departmentId" label="部门">
				<DepartmentTreeSelect
					:disabled="state.formDisabled || !state.formData?.orgId"
					:org-id="state.formData.orgId"
					v-model="state.formData.departmentId"
					v-model:department-name="state.formData.departmentName"
				/>
			</FaFormItem>
			<FaFormItem prop="sex" label="部门负责人">
				<RadioGroup name="BooleanEnum" v-model="state.formData.isPrincipal" />
			</FaFormItem>
			<FaFormItem prop="positionId" label="职位">
				<FaSelect
					:request-api="positionApi.positionSelector"
					v-model="state.formData.positionId"
					v-model:label="state.formData.positionName"
					placeholder="请选择职位"
					clearable
				/>
			</FaFormItem>
			<FaFormItem prop="jobLevelId" label="职级">
				<FaSelect
					:request-api="jobLevelApi.jobLevelSelector"
					v-model="state.formData.jobLevelId"
					v-model:label="state.formData.jobLevelName"
					placeholder="请选择职级"
					clearable
				/>
			</FaFormItem>

			<FaLayoutGridItem span="3">
				<el-divider content-position="left">基础档案</el-divider>
			</FaLayoutGridItem>
			<FaFormItem prop="employeeName" label="职员名称">
				<el-input v-model="state.formData.employeeName" maxlength="20" placeholder="请输入职员名称" />
			</FaFormItem>
			<FaFormItem prop="mobile" label="手机">
				<el-input
					v-model="state.formData.mobile"
					maxlength="11"
					placeholder="请输入手机"
					autocapitalize="off"
					autocomplete="tel"
					inputmode="tel"
					spellcheck="false"
				/>
			</FaFormItem>
			<FaFormItem prop="email" label="邮箱">
				<el-input
					v-model="state.formData.email"
					maxlength="50"
					placeholder="请输入邮箱"
					autocapitalize="off"
					autocomplete="email"
					inputmode="email"
					spellcheck="false"
				/>
			</FaFormItem>
			<FaFormItem prop="sex" label="性别">
				<RadioGroup name="GenderEnum" v-model="state.formData.sex" />
			</FaFormItem>
			<FaFormItem prop="entryDate" label="入职日期">
				<el-date-picker
					type="date"
					v-model="state.formData.entryDate"
					:disabled-date="isDateAfterNow"
					value-format="YYYY-MM-DD"
					placeholder="请选择入职日期"
				/>
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input type="textarea" v-model="state.formData.remark" :rows="2" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
			<FaFormItem prop="idPhoto" label="证件照">
				<FaUploadImage v-model="state.formData.idPhoto" :upload-api="fileApi.uploadIdPhoto" />
			</FaFormItem>

			<FaLayoutGridItem span="3">
				<el-divider content-position="left">角色信息</el-divider>
			</FaLayoutGridItem>
			<FaFormItem prop="roleList" label="角色" span="3">
				<el-checkbox-group v-model="state.formData.roleIds" @change="handleRoleChange">
					<el-checkbox v-for="(item, index) of state.roleList" :key="index" :value="item.value">
						{{ item.label }}
					</el-checkbox>
				</el-checkbox-group>
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage, dayjs } from "element-plus";
import { type ElSelectorOutput, type FaDialogInstance, type FaFormInstance, RegExps } from "fast-element-plus";
import { isDateAfterNow, withDefineType } from "@fast-china/utils";
import { GenderEnum } from "@/api/enums/GenderEnum";
import { employeeApi } from "@/api/services/Admin/employee";
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import { organizationApi } from "@/api/services/Admin/organization";
import { positionApi } from "@/api/services/Admin/position";
import { roleApi } from "@/api/services/Admin/role";
import { fileApi } from "@/api/services/File";
import type { CheckboxValueType, FormRules } from "element-plus";
import type { AddEmployeeInput } from "@/api/services/Admin/employee/models/AddEmployeeInput";
import type { EditEmployeeInput } from "@/api/services/Admin/employee/models/EditEmployeeInput";
import type { QueryEmployeeDetailOutput } from "@/api/services/Admin/employee/models/QueryEmployeeDetailOutput";

defineOptions({
	name: "AdminEmployeeEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

type IFormData = EditEmployeeInput &
	AddEmployeeInput & {
		roleIds?: string[];
	};

const state = reactive({
	formData: withDefineType<IFormData>({
		roleIds: [],
	}),
	formRules: withDefineType<FormRules<IFormData>>({
		orgId: [{ required: true, message: "请选择机构", trigger: "change" }],
		departmentId: [{ required: true, message: "请选择部门", trigger: "change" }],
		positionId: [{ required: true, message: "请选择职位", trigger: "change" }],
		jobLevelId: [{ required: true, message: "请选择职级", trigger: "change" }],
		employeeName: [{ required: true, message: "请输入职员名称", trigger: "blur" }],
		mobile: [
			{ required: true, message: "请输入手机", trigger: "blur" },
			{ pattern: RegExps.Mobile, message: "请输入正确的手机号", trigger: "blur" },
		],
		email: [
			{ required: true, message: "请输入邮箱", trigger: "blur" },
			{ pattern: RegExps.Email, message: "请输入正确的邮箱", trigger: "blur" },
			{ max: 50, message: "邮箱不能超过50位字符", trigger: "blur" },
		],
		idPhoto: [{ required: true, message: "请上传证件照", trigger: "change" }],
		entryDate: [{ required: true, message: "请选择入职日期", trigger: "change" }],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "职员",
	roleList: withDefineType<ElSelectorOutput<string>[]>([]),
});

/** 从 API 详情响应中提取主部门信息，展平到 formData */
const flattenPrimaryOrg = (apiRes: QueryEmployeeDetailOutput) => {
	const primaryOrg = apiRes.orgList?.find((item) => item.isPrimary) ?? apiRes.orgList?.[0];
	if (!primaryOrg) return;

	state.formData.orgId = primaryOrg.orgId;
	state.formData.orgName = primaryOrg.orgName;
	state.formData.departmentId = primaryOrg.departmentId;
	state.formData.departmentName = primaryOrg.departmentName;
	state.formData.positionId = primaryOrg.positionId;
	state.formData.positionName = primaryOrg.positionName;
	state.formData.jobLevelId = primaryOrg.jobLevelId;
	state.formData.jobLevelName = primaryOrg.jobLevelName;
	state.formData.isPrincipal = primaryOrg.isPrincipal;
};

/** 将 UI 展平的单部门字段组装回 orgList 数组，用于提交 API */
const buildOrgList = () => {
	return [
		{
			orgId: state.formData.orgId,
			departmentId: state.formData.departmentId,
			isPrimary: true,
			positionId: state.formData.positionId,
			jobLevelId: state.formData.jobLevelId,
			isPrincipal: state.formData.isPrincipal ?? false,
		},
	];
};

const handleRoleChange = (val: CheckboxValueType[]) => {
	state.formData.roleList = val
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

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		switch (state.dialogState) {
			case "add":
				await employeeApi.addEmployee(state.formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await employeeApi.editEmployee({
					...state.formData,
					orgList: buildOrgList(),
				});
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const detail = (employeeId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await employeeApi.queryEmployeeDetail(employeeId);
		state.formData = apiRes;
		state.formData.roleIds = apiRes.roleList.map((m) => m.roleId);
		flattenPrimaryOrg(apiRes);
		state.dialogTitle = `职员详情 - ${apiRes.employeeName}`;
		state.roleList = await roleApi.roleSelector();
	});
};

const add = () => {
	faDialogRef.value.open(async () => {
		state.dialogState = "add";
		state.dialogTitle = "添加职员";
		state.formDisabled = false;
		state.formData = {
			isPrincipal: false,
			sex: GenderEnum.Unknown,
			entryDate: dayjs().format("YYYY-MM-DD"),
			roleList: [],
		};
		state.roleList = await roleApi.roleSelector();
	});
};

const edit = (employeeId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		const apiRes = await employeeApi.queryEmployeeDetail(employeeId);
		state.formData = apiRes;
		state.formData.roleIds = apiRes.roleList.map((m) => m.roleId);
		flattenPrimaryOrg(apiRes);
		state.dialogTitle = `编辑职员 - ${apiRes.employeeName}`;
		state.roleList = await roleApi.roleSelector();
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>

<style lang="scss" scoped>
.el-table__cell .el-form-item {
	margin-bottom: 0;
}
</style>
