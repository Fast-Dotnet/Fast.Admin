<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-cell-group title="基础信息" border>
				<wd-cell title="工号" :placeholder="state.formData.employeeNo" />
				<wd-cell title="入职日期" :placeholder="state.formData.entryDate?.slice(0, 10) || '无'" />

				<wd-form-item title="姓名" prop="employeeName">
					<wd-input v-model="state.formData.employeeName" clearable :maxlength="20" placeholder="请输入姓名" />
				</wd-form-item>

				<wd-form-item title="性别" prop="sex" is-link :value="sexLabel" placeholder="请选择性别" @click="state.sexVisible = true" />
				<wd-picker v-model="sex" v-model:visible="state.sexVisible" :columns="genderEnum" />

				<wd-form-item title="联系电话" prop="mobile">
					<wd-input v-model="state.formData.mobile" clearable inputmode="tel" :maxlength="11" placeholder="请输入联系电话" />
				</wd-form-item>

				<wd-form-item title="联系邮箱" prop="email">
					<wd-input v-model="state.formData.email" clearable inputmode="email" :maxlength="50" placeholder="请输入联系邮箱" />
				</wd-form-item>

				<wd-form-item title="证件照" prop="idPhoto" is-link>
					<image class="image__idPhoto" :src="state.formData.idPhoto" @click="handleIdPhoto" />
				</wd-form-item>
			</wd-cell-group>

			<wd-cell-group
				v-for="org in state.formData.orgList"
				:key="org.departmentId"
				class="mt40"
				:title="org.isPrimary ? '主要任职' : '兼任信息'"
				border
			>
				<wd-cell title="机构" :placeholder="org.orgName" />
				<wd-cell title="部门" :placeholder="org.departmentName" />
				<wd-cell title="职位" :placeholder="org.positionName" />
				<wd-cell title="职级" :placeholder="org.jobLevelName" />
				<wd-cell title="负责人" :placeholder="org.isPrincipal ? '是' : '否'" />
			</wd-cell-group>

			<wd-button custom-class="btn__confirm" type="primary" @click="handleConfirm">保存</wd-button>
		</wd-form>
	</view>
</template>

<script setup lang="ts">
import { onLoad } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { GenderEnum } from "@/api/enums/GenderEnum";
import { employeeApi } from "@/api/services/Admin/employee";
import { fileApi } from "@/api/services/File";
import { RegExps } from "@/constants";
import { useFormSchema, useLoading, useToast } from "@/hooks";
import { useApp, useUserInfo } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { PickerOption } from "@wot-ui/ui/components/wd-picker-view/types";
import type { EditEmployeeInput } from "@/api/services/Admin/employee/models/EditEmployeeInput";
import type { QueryEmployeeDetailOutput } from "@/api/services/Admin/employee/models/QueryEmployeeDetailOutput";

definePage({
	name: "SettingEmployee",
	layout: "layout",
	style: {
		navigationBarTitleText: "个人资料",
	},
});

const appStore = useApp();
const userInfoStore = useUserInfo();

const formRef = ref<FormInstance>();

const genderEnum = appStore.getDictionary<PickerOption>("GenderEnum");
const state = reactive({
	formData: withDefineType<QueryEmployeeDetailOutput>({
		sex: GenderEnum.Unknown,
	}),
	formSchema: useFormSchema<EditEmployeeInput>({
		employeeName: { required: true, message: "请输入姓名" },
		mobile: [
			{ required: true, message: "请输入联系电话" },
			{ pattern: RegExps.Mobile, message: "请输入有效的手机号" },
		],
		email: [
			{ required: true, message: "请输入联系邮箱" },
			{ pattern: RegExps.Email, message: "请输入有效的邮箱" },
		],
		sex: { required: true, message: "请选择性别" },
		idPhoto: { required: true, message: "请上传证件照" },
	}),
	sexVisible: false,
});

const sex = computed({
	get: () => [state.formData.sex],
	set: (value) => {
		state.formData.sex = value[0];
	},
});

const sexLabel = computed(() => genderEnum.find((item) => item.value === state.formData.sex)?.label);

/** 处理证件照 */
const handleIdPhoto = throttle(async () => {
	const result = await uni.chooseImage({
		count: 1,
		sizeType: ["compressed"],
	});
	const idPhoto = await fileApi.uploadAvatar(result.tempFilePaths[0]);
	if (!idPhoto) return;
	state.formData.idPhoto = idPhoto;
});

/** 提交 */
const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
	await employeeApi.editSelfEmployee(state.formData).finally(() => useLoading.hide());
	useToast.success("修改成功！");
	// 刷新用户信息
	await userInfoStore.refreshUserInfo();
});

onLoad(() => {
	useLoading.show();
	employeeApi
		.querySelfEmployeeDetail()
		.then((apiRes) => {
			state.formData = apiRes;
		})
		.finally(() => useLoading.hide());
});
</script>

<style lang="scss" scoped>
:deep(.wd-cell__wrapper) {
	align-items: center;
}

.image__idPhoto {
	display: inline-flex;
	justify-content: right;
	width: 120rpx;
	height: 120rpx;
	border-radius: 50%;
}

.btn__confirm {
	display: block;
	margin: 30rpx 50rpx;
}
</style>
