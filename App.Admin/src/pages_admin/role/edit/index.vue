<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="角色类型" prop="roleType">
				<FaRadioGroup name="RoleTypeEnum" v-model="state.formData.roleType" type="button" />
			</wd-form-item>
			<wd-form-item title="系统菜单" prop="isSystemMenu">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.isSystemMenu" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="角色名称" prop="roleName">
				<wd-input v-model="state.formData.roleName" clearable :maxlength="20" placeholder="请输入角色名称" />
			</wd-form-item>
			<wd-form-item title="角色编码" prop="roleCode">
				<wd-input v-model="state.formData.roleCode" clearable :maxlength="30" placeholder="请输入角色编码" />
			</wd-form-item>
			<wd-form-item title="排序" prop="sort">
				<wd-input v-model="state.formData.sort" type="number" placeholder="请输入排序" />
			</wd-form-item>
			<wd-form-item title="数据范围" prop="dataScopeType">
				<FaRadioGroup name="DataScopeTypeEnum" v-model="state.formData.dataScopeType" />
			</wd-form-item>
			<FaDepartmentTreeSelect
				v-if="state.formData.dataScopeType === DataScopeTypeEnum.CustomDept"
				title="自定义部门"
				v-model="state.formData.dataScopeDepartmentIds"
				prop="dataScopeDepartmentIds"
				multiple
			/>
			<wd-form-item title="可分配角色" prop="assignableRoleIds">
				<wd-checkbox-group v-model="state.formData.assignableRoleIds" type="square" direction="horizontal">
					<wd-checkbox v-for="role in state.roleList" :key="role.value" :name="role.value" :disabled="role.disabled">
						{{ role.label }}
					</wd-checkbox>
				</wd-checkbox-group>
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
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";
import { roleApi } from "@/api/services/Admin/role";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddRoleInput } from "@/api/services/Admin/role/models/AddRoleInput";
import type { EditRoleInput } from "@/api/services/Admin/role/models/EditRoleInput";

definePage({
	name: "AdminRoleEdit",
	authCode: ["Role:Add", "Role:Edit"],
	style: {
		navigationBarTitleText: "角色",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<AddRoleInput & EditRoleInput>({}),
	formSchema: useFormSchema<AddRoleInput & EditRoleInput>({
		roleName: { required: true, message: "请输入角色名称" },
		roleCode: { required: true, message: "请输入角色编码" },
		sort: {
			required: true,
			message: "请输入排序",
			validator: (value) => (Number.isInteger(value) && value >= 1 && value <= 9999 ? undefined : "排序必须是 1 到 9999 之间的整数"),
		},
		dataScopeDepartmentIds: { required: true, message: "请选择自定义部门" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	roleId: withDefineType<string>(),
	roleList: withDefineType<ElSelectorOutput<string>[]>([]),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await roleApi.addRole(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await roleApi.editRole(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_admin/role/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				{
					uni.setNavigationBarTitle({ title: "添加角色" });
					state.formData = {
						roleType: RoleTypeEnum.Admin,
						isSystemMenu: true,
						dataScopeType: DataScopeTypeEnum.All,
						dataScopeDepartmentIds: [],
						assignableRoleIds: [],
						sort: 1,
					};
				}
				break;
			case "edit":
				{
					const apiRes = await roleApi.queryRoleDetail(state.roleId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑角色 - ${apiRes.roleName}` });
				}
				break;
		}
		state.roleList = await roleApi.roleSelector();
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; roleId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.roleId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.roleId = options.roleId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
