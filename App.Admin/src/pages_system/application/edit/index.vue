<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="版本" prop="edition">
				<FaRadioGroup name="EditionEnum" v-model="state.formData.edition" type="button" />
			</wd-form-item>
			<wd-form-item title="应用名称" prop="appName">
				<wd-input v-model="state.formData.appName" clearable :maxlength="30" placeholder="请输入应用名称" />
			</wd-form-item>
			<wd-form-item title="主题色" prop="themeColor">
				<wd-input v-model="state.formData.themeColor" clearable :maxlength="7" placeholder="请输入主题色，如 #4488FE" />
			</wd-form-item>
			<FaTenantSelect v-model="state.formData.tenantId" v-model:tenant-name="state.formData.tenantName" prop="tenantId" />
			<wd-form-item title="备注" prop="remark">
				<wd-textarea
					v-model="state.formData.remark"
					custom-style="--fa-textarea-height: 75px"
					clearable
					:maxlength="200"
					placeholder="请输入备注"
				/>
			</wd-form-item>
			<wd-form-item title="Logo" prop="logoUrl">
				<FaUploadImage v-model="state.formData.logoUrl" :upload-api="fileApi.uploadLogo" />
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
import { EditionEnum } from "@/api/enums/EditionEnum";
import { applicationApi } from "@/api/services/Center/application";
import { fileApi } from "@/api/services/File";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddApplicationInput } from "@/api/services/Center/application/models/AddApplicationInput";
import type { EditApplicationInput } from "@/api/services/Center/application/models/EditApplicationInput";

definePage({
	name: "SystemApplicationEdit",
	authCode: ["App:Add", "App:Edit"],
	style: {
		navigationBarTitleText: "应用",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<AddApplicationInput & EditApplicationInput>({}),
	formSchema: useFormSchema<AddApplicationInput & EditApplicationInput>({
		appName: { required: true, message: "请输入应用名称" },
		themeColor: [
			{ required: true, message: "请输入主题色" },
			{ pattern: /^#[0-9a-fA-F]{6}$/, message: "请输入正确的主题色，如 #4488FE" },
		],
		logoUrl: { required: true, message: "请上传Logo" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	appId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await applicationApi.addApplication(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await applicationApi.editApplication(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Application");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/application/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				uni.setNavigationBarTitle({ title: "添加应用" });
				state.formData = {
					edition: EditionEnum.None,
				};
				break;
			case "edit":
				{
					const apiRes = await applicationApi.queryApplicationDetail(state.appId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑应用 - ${apiRes.appName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; appId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.appId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.appId = options.appId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
