<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<FaCellPicker title="机构" prop="orgId" :value="state.formData.orgName" placeholder="请选择机构">
				<template #default="{ visible }">
					<FaTreePicker
						title="机构选择"
						v-model="state.formData.orgId"
						v-model:label="state.formData.orgName"
						v-model:visible="visible.value"
						:request-api="organizationApi.organizationSelector"
						disabled-search
						clearable
						check-strictly
						@change="
							() => {
								state.formData.parentId = null;
								state.formData.parentName = null;
							}
						"
						@clear="
							() => {
								state.formData.parentId = null;
								state.formData.parentName = null;
							}
						"
					/>
				</template>
			</FaCellPicker>
			<FaDepartmentTreeSelect
				title="父级"
				v-model="state.formData.parentId"
				v-model:department-name="state.formData.parentName"
				prop="parentId"
				:org-id="state.formData.orgId"
				:disabled="!state.formData.orgId"
				placeholder="请选择父级部门"
			/>
			<wd-form-item title="部门名称" prop="departmentName">
				<wd-input v-model="state.formData.departmentName" clearable :maxlength="20" placeholder="请输入部门名称" />
			</wd-form-item>
			<wd-form-item title="部门编码" prop="departmentCode">
				<wd-input v-model="state.formData.departmentCode" clearable :maxlength="30" placeholder="请输入部门编码" />
			</wd-form-item>
			<wd-form-item title="排序" prop="sort">
				<wd-input v-model="state.formData.sort" type="number" placeholder="请输入排序" />
			</wd-form-item>
			<wd-form-item title="数据公开" prop="dataPublic">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.dataPublic" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="联系人" prop="contacts">
				<wd-input v-model="state.formData.contacts" clearable :maxlength="20" placeholder="请输入联系人" />
			</wd-form-item>
			<wd-form-item title="电话" prop="phone">
				<wd-input v-model="state.formData.phone" clearable :maxlength="20" inputmode="tel" placeholder="请输入电话" />
			</wd-form-item>
			<wd-form-item title="邮箱" prop="email">
				<wd-input v-model="state.formData.email" clearable :maxlength="50" inputmode="email" placeholder="请输入邮箱" />
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
import { departmentApi } from "@/api/services/Admin/department";
import { organizationApi } from "@/api/services/Admin/organization";
import { RegExps } from "@/constants";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddDepartmentInput } from "@/api/services/Admin/department/models/AddDepartmentInput";
import type { EditDepartmentInput } from "@/api/services/Admin/department/models/EditDepartmentInput";

definePage({
	name: "AdminDepartmentEdit",
	authCode: ["Department:Add", "Department:Edit"],
	style: {
		navigationBarTitleText: "部门",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();

type IFormData = EditDepartmentInput &
	AddDepartmentInput & {
		orgName?: string;
		parentName?: string;
	};

const state = reactive({
	loading: false,
	formData: withDefineType<IFormData>({}),
	formSchema: useFormSchema<IFormData>({
		orgId: { required: true, message: "请选择机构" },
		departmentName: { required: true, message: "请输入部门名称" },
		departmentCode: { required: true, message: "请输入部门编码" },
		sort: {
			required: true,
			message: "请输入排序",
			validator: (value) => (Number.isInteger(value) && value >= 1 && value <= 9999 ? undefined : "排序必须是 1 到 9999 之间的整数"),
		},
		dataPublic: { required: true, message: "请选择数据公开" },
		email: [
			{ pattern: RegExps.Email, message: "请输入正确的邮箱" },
			{ validator: (value) => (value?.length > 50 ? "邮箱不能超过50位字符" : undefined) },
		],
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	departmentId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await departmentApi.addDepartment(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await departmentApi.editDepartment(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Department");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_admin/department/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				{
					uni.setNavigationBarTitle({ title: "添加部门" });
					state.formData = {
						sort: 1,
						dataPublic: false,
					};
				}
				break;
			case "edit":
				{
					const apiRes = await departmentApi.queryDepartmentDetail(state.departmentId);
					if (apiRes.parentId === "0") {
						apiRes.parentId = undefined;
					}
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑部门 - ${apiRes.departmentName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; departmentId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.departmentId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.departmentId = options.departmentId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
