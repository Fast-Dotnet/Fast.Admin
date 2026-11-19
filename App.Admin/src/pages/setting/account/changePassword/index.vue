<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />

		<wd-empty v-if="state.errorMessage" icon="no-content" :tip="state.errorMessage">
			<template #bottom>
				<wd-button type="info" @click="handleLoad">重新加载</wd-button>
			</template>
		</wd-empty>
		<template v-else>
			<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" :disabled="state.loading" border>
				<wd-form-item title="旧密码" prop="oldPassword">
					<wd-input
						v-model="state.formData.oldPassword"
						:show-word-limit="false"
						show-password
						:maxlength="20"
						placeholder="请输入旧密码"
						clearable
					/>
				</wd-form-item>
				<wd-form-item title="新密码" prop="newPassword">
					<wd-input
						v-model="state.formData.newPassword"
						:show-word-limit="false"
						show-password
						:maxlength="20"
						placeholder="8~20位，包含大小写字母和数字"
						clearable
					/>
				</wd-form-item>
				<wd-form-item title="确认密码" prop="confirmPassword">
					<wd-input
						v-model="state.formData.confirmPassword"
						:show-word-limit="false"
						show-password
						:maxlength="20"
						placeholder="8~20位，包含大小写字母和数字"
						clearable
					/>
				</wd-form-item>
			</wd-form>

			<wd-button custom-class="fa-form-submit" type="primary" :loading="state.loading" @click="handleConfirm">修改密码</wd-button>
		</template>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import { RegExps } from "@/constants";
import { useFormSchema, useMessageBox } from "@/hooks";
import { useUserInfo } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { ChangePasswordInput } from "@/api/services/Center/account/models/ChangePasswordInput";

definePage({
	name: "ChangePassword",
	style: {
		navigationBarTitleText: "修改密码",
		enablePullDownRefresh: true,
	},
});

const userInfoStore = useUserInfo();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	errorMessage: withDefineType<string>(),
	formData: withDefineType<ChangePasswordInput>({}),
	formSchema: useFormSchema<ChangePasswordInput>({
		oldPassword: [{ required: true, message: "请输入旧密码" }],
		newPassword: [
			{ required: true, message: "请输入新密码" },
			{ pattern: RegExps.MediumPassword, message: "密码为8~20位，必须包含大小写字母和数字" },
		],
		confirmPassword: [
			{ required: true, message: "请再次输入新密码" },
			{ validator: (value, model) => (value !== model.newPassword ? "两次密码输入不一致" : undefined) },
		],
	}),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		await accountApi.changePassword(state.formData);
		await useMessageBox.alert({ msg: "密码修改成功，请重新登录", confirmButtonText: "重新登录", closeOnClickModal: false });
		await userInfoStore.logout();
	} finally {
		state.loading = false;
	}
});

const handleLoad = async () => {
	state.errorMessage = undefined;
	state.loading = true;
	try {
		const apiRes = await accountApi.queryEditAccountDetail();
		state.formData = {
			rowVersion: apiRes.rowVersion,
		};
	} catch {
		state.errorMessage = "账号信息加载失败，请重试";
	} finally {
		state.loading = false;
	}
};

onLoad(() => {
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
