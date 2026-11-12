<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="配置编码" prop="configCode">
				<wd-input
					v-if="state.pageMode === 'add'"
					v-model="state.formData.configCode"
					clearable
					:maxlength="50"
					placeholder="请输入配置编码"
				/>
				<text v-else>{{ state.formData.configCode }}</text>
			</wd-form-item>
			<wd-form-item title="配置名称" prop="configName">
				<wd-input v-model="state.formData.configName" clearable :maxlength="50" placeholder="请输入配置名称" />
			</wd-form-item>
			<wd-form-item title="配置值" prop="configValue">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 150rpx"
					v-model="state.formData.configValue"
					clearable
					:maxlength="500"
					placeholder="请输入配置值"
				/>
			</wd-form-item>
			<wd-form-item title="备注" prop="remark">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 150rpx"
					v-model="state.formData.remark"
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
import { configApi } from "@/api/services/Center/config";
import { useFormSchema, useLoading, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddConfigInput } from "@/api/services/Center/config/models/AddConfigInput";
import type { EditConfigInput } from "@/api/services/Center/config/models/EditConfigInput";

definePage({
	name: "DevConfigEdit",
	authCode: "Config:Edit",
	style: {
		navigationBarTitleText: "配置",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<EditConfigInput & AddConfigInput>({}),
	formSchema: useFormSchema<EditConfigInput & AddConfigInput>({
		configCode: { required: true, message: "请输入配置编码" },
		configName: { required: true, message: "请输入配置名称" },
		configValue: { required: true, message: "请输入配置值" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	configId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
	try {
		switch (state.pageMode) {
			case "add":
				await configApi.addConfig(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await configApi.editConfig(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		useLoading.hide();
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_dev/config/page/index");
});

const handleLoad = async () => {
	switch (state.pageMode) {
		case "add":
			uni.setNavigationBarTitle({ title: "添加配置" });
			break;
		case "edit":
			{
				const apiRes = await configApi.queryConfigDetail(state.configId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `编辑配置 - ${apiRes.configName}` });
			}
			break;
	}
};

onLoad((options: { mode?: IPageStateType; configId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.configId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.configId = options.configId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
