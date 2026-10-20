<template>
	<view class="page">
		<wd-form ref="formRef" :model="userInfo" :schema="state.formSchema" border>
			<wd-cell-group border>
				<wd-form-item title="头像" prop="avatar" is-link>
					<!-- #ifdef MP-WEIXIN -->
					<button class="btn__avatar" :plain="true" openType="chooseAvatar" @chooseavatar="handleAvatar">
						<image class="image__avatar" :src="userInfo.avatar" />
					</button>
					<!-- #endif -->
					<!-- #ifndef MP-WEIXIN -->
					<image class="image__avatar" :src="userInfo.avatar" @click="handleAvatar" />
					<!-- #endif -->
				</wd-form-item>

				<wd-form-item title="昵称" prop="nickName">
					<wd-input v-model="userInfo.nickName" clearable type="nickname" placeholder="请输入昵称" />
				</wd-form-item>

				<wd-form-item title="手机号" prop="mobile" :disabled="!userInfo.allowModifyMobile">
					<!-- #ifdef MP-WEIXIN -->
					<button class="btn__mobile" :plain="true" openType="getRealtimePhoneNumber" @getphonenumber="handlePhone">
						{{ userInfo.mobile || "暂未授权手机号" }}
					</button>
					<!-- #endif -->
					<!-- #ifndef MP-WEIXIN -->
					<wd-input v-model="userInfo.mobile" clearable type="tel" :maxlength="11" placeholder="请输入手机号" />
					<!-- #endif -->
				</wd-form-item>
				<wd-cell title="手机号更新时间" :value="userInfo.mobileUpdateTime" />

				<wd-form-item title="性别" prop="sex" is-link @click="state.sexVisible = true">
					<wd-picker
						v-model="state.sex"
						v-model:visible="state.sexVisible"
						:columns="genderEnum"
						@confirm="
							({ value }: { value: GenderEnum }) => {
								userInfo.sex = value;
							}
						"
					/>
				</wd-form-item>

				<wd-cell title="最后登录设备" :value="userInfo.lastLoginDevice" />
				<wd-cell title="最后登录省份" :value="userInfo.lastLoginProvince" />
				<wd-cell title="最后登录城市" :value="userInfo.lastLoginCity" />
				<wd-cell title="最后登录Ip" :value="userInfo.lastLoginIp" />
				<wd-cell title="最后登录时间" :value="userInfo.lastLoginTime" />
				<wd-cell title="创建时间" :value="userInfo.createdTime" />
				<wd-cell title="更新时间" :value="userInfo.updatedTime" />
			</wd-cell-group>

			<wd-button custom-class="btn__confirm" type="danger" @click="handleConfirm">保存</wd-button>
		</wd-form>
	</view>
</template>

<script setup lang="ts">
import { onLoad } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { logger, throttle, withDefineType } from "@fast-china/utils";
import { weChatApi } from "@/api/services/Auth/weChat";
import { clientUserApi } from "@/api/services/Center/clientUser";
import { fileApi } from "@/api/services/File";
import { RegExps } from "@/constants";
import { useFormSchema, useLoading, useToast } from "@/hooks";
import { useApp, useUserInfo } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { PickerOption } from "@wot-ui/ui/components/wd-picker-view/types";
import type { GenderEnum } from "@/api/enums/GenderEnum";
import type { EditClientUserInput } from "@/api/services/Center/clientUser/models/EditClientUserInput";
import type { QueryClientUserDetailOutput } from "@/api/services/Center/clientUser/models/QueryClientUserDetailOutput";

definePage({
	name: "SettingUserInfo",
	layout: "layout",
	style: {
		navigationBarTitleText: "个人资料",
	},
});

const appStore = useApp();
const userInfoStore = useUserInfo();

const formRef = ref<FormInstance>();

const genderEnum = appStore.getDictionary<PickerOption>("GenderEnum");

const userInfo = reactive(withDefineType<QueryClientUserDetailOutput>({}));
const state = reactive({
	formSchema: useFormSchema<EditClientUserInput>({
		avatar: { required: true, message: "请选择头像" },
		nickName: { required: true, message: "请输入昵称" },
		mobile: [
			{ required: true, message: "请输入手机号" },
			{ pattern: RegExps.Mobile, message: "请输入有效的手机号" },
		],
		sex: { required: true, message: "请选择性别" },
	}),
	sex: computed({
		get: () => [userInfo.sex],
		set: (value) => {
			userInfo.sex = value[0];
		},
	}),
	sexVisible: false,
	sexLabel: computed(() => genderEnum.find((item) => item.value === userInfo.sex)?.label || ""),
});

/** 头像上传 */
const handleAvatarUpload = async (url: string) => {
	const avatar = await fileApi.uploadAvatar(url);
	if (!avatar) return;
	userInfo.avatar = avatar;
};

/** 处理头像 */
const handleAvatar = throttle(async (event?: UniHelper.ButtonOnChooseavatarEvent) => {
	// #ifdef MP-WEIXIN
	const { avatarUrl } = event.detail;
	if (!avatarUrl) {
		useToast.error("头像获取失败");
		return;
	}
	await handleAvatarUpload(avatarUrl);
	//#endif

	// #ifndef MP-WEIXIN
	uni.chooseImage({
		count: 1,
		sizeType: ["compressed"],
		success: async (res) => {
			await handleAvatarUpload(res.tempFilePaths[0]);
		},
	});
	//#endif
});

/** 处理手机号 */
const handlePhone = async (event?: UniHelper.ButtonOnGetrealtimephonenumberEvent) => {
	// #ifdef MP-WEIXIN
	logger.log("GetRealtimePhoneNumber", event.detail);
	const { code } = event.detail;
	if (!code) return;
	// 解析微信手机号
	const apiRes = await weChatApi.weChatCode2PhoneNumber({
		code,
	});
	userInfo.mobile = apiRes.purePhoneNumber;
	//#endif
};

/** 提交 */
const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	const { mobile, nickName, avatar, sex, rowVersion } = userInfo;
	useLoading.show();
	await clientUserApi
		.editClientUser({
			mobile,
			nickName,
			avatar,
			sex,
			rowVersion,
		})
		.finally(() => useLoading.hide());
	useToast.success("修改成功！");
	// 刷新用户信息
	await userInfoStore.refreshUserInfo();
});

onLoad(() => {
	useLoading.show();
	clientUserApi
		.queryClientUserDetail()
		.then((apiRes) => {
			Object.assign(userInfo, apiRes);
		})
		.finally(() => useLoading.hide());
});
</script>

<style lang="scss" scoped>
.btn__avatar,
.btn__mobile {
	width: 100%;
	display: inline-flex;
	justify-content: right;
	padding: 0;
	background: transparent;
	border: 0;
}

.image__avatar {
	width: 120rpx;
	height: 120rpx;
	border-radius: 50%;
}

.btn__confirm {
	display: block;
	margin: 30rpx 50rpx;
}
</style>
