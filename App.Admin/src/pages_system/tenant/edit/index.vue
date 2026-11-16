<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="租户名称" prop="tenantName">
				<wd-input
					v-model="state.formData.tenantName"
					clearable
					:maxlength="30"
					placeholder="请输入租户名称"
					@update:model-value="handleSpellName"
				/>
			</wd-form-item>
			<wd-form-item title="租户编码" prop="tenantCode">
				<wd-input v-model="state.formData.tenantCode" clearable :maxlength="5" placeholder="请输入租户编码" />
			</wd-form-item>
			<wd-form-item title="租户简称" prop="shortName">
				<wd-input v-model="state.formData.shortName" clearable :maxlength="20" placeholder="请输入租户简称" />
			</wd-form-item>
			<wd-form-item title="租户英文名称" prop="spellName">
				<wd-input v-model="state.formData.spellName" clearable :maxlength="100" placeholder="请输入租户英文名称" />
			</wd-form-item>
			<wd-form-item title="版本" prop="edition">
				<FaRadioGroup name="EditionEnum" v-model="state.formData.edition" type="button" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="机器人名称" prop="robotName">
				<wd-input v-model="state.formData.robotName" clearable :maxlength="20" placeholder="请输入机器人名称" />
			</wd-form-item>
			<wd-form-item v-if="state.pageMode === 'edit'" title="状态" prop="status">
				<FaRadioGroup name="CommonStatusEnum" v-model="state.formData.status" type="button" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="Logo" prop="logoUrl">
				<FaUploadImage v-model="state.formData.logoUrl" :upload-api="fileApi.uploadLogo" />
			</wd-form-item>

			<wd-cell-group title="管理员信息" border>
				<wd-form-item title="管理员名称" prop="adminName">
					<wd-input v-model="state.formData.adminName" clearable :maxlength="20" placeholder="请输入管理员名称" />
				</wd-form-item>
				<wd-form-item title="管理员手机" prop="adminMobile">
					<wd-input v-model="state.formData.adminMobile" clearable :maxlength="11" inputmode="tel" placeholder="请输入管理员手机" />
				</wd-form-item>
				<wd-form-item title="管理员邮箱" prop="adminEmail">
					<wd-input v-model="state.formData.adminEmail" clearable :maxlength="50" inputmode="email" placeholder="请输入管理员邮箱" />
				</wd-form-item>
				<wd-form-item title="管理员电话" prop="adminPhone">
					<wd-input v-model="state.formData.adminPhone" clearable :maxlength="20" inputmode="tel" placeholder="请输入管理员电话" />
				</wd-form-item>
			</wd-cell-group>

			<wd-cell-group title="阿里云" border>
				<wd-form-item title="签名名称" prop="aliyunSignName">
					<wd-input v-model="state.formData.aliyunSignName" clearable :maxlength="30" placeholder="请输入阿里云签名名称" />
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
import { pinyin } from "pinyin-pro";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { tenantApi } from "@/api/services/Center/tenant";
import { fileApi } from "@/api/services/File";
import { RegExps } from "@/constants";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddTenantInput } from "@/api/services/Center/tenant/models/AddTenantInput";
import type { EditTenantInput } from "@/api/services/Center/tenant/models/EditTenantInput";

definePage({
	name: "SystemTenantEdit",
	authCode: ["Tenant:Add", "Tenant:Edit"],
	style: {
		navigationBarTitleText: "租户",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<AddTenantInput & EditTenantInput>({
		edition: EditionEnum.Trial,
		status: CommonStatusEnum.Enable,
	}),
	formSchema: useFormSchema<AddTenantInput & EditTenantInput>({
		tenantName: { required: true, message: "请输入租户名称" },
		tenantCode: { required: true, message: "请输入租户编码" },
		shortName: { required: true, message: "请输入租户简称" },
		spellName: { required: true, message: "请输入租户英文名称" },
		adminName: { required: true, message: "请输入管理员名称" },
		adminMobile: [
			{ required: true, message: "请输入管理员手机" },
			{ pattern: RegExps.Mobile, message: "请输入正确的手机号" },
		],
		adminEmail: [
			{ required: true, message: "请输入管理员邮箱" },
			{ pattern: RegExps.Email, message: "请输入正确的邮箱" },
		],
		robotName: { required: true, message: "请输入机器人名称" },
		logoUrl: { required: true, message: "请上传Logo" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	tenantId: withDefineType<string>(),
});

const handleSpellName = (value: string) => {
	state.formData.spellName = pinyin(value, { toneType: "none", type: "array" })
		.map((item) => item.charAt(0).toUpperCase() + item.slice(1))
		.join("");
};

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await tenantApi.addTenant(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await tenantApi.editTenant(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/tenant/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				uni.setNavigationBarTitle({ title: "添加租户" });
				break;
			case "edit":
				{
					const apiRes = await tenantApi.queryTenantDetail(state.tenantId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑租户 - ${apiRes.tenantName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; tenantId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.tenantId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.tenantId = options.tenantId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
