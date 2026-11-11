<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" :disabled="state.formDisabled" border>
			<wd-form-item title="配置编码" prop="configCode">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 150rpx"
					v-model="state.formData.configCode"
					clearable
					:maxlength="50"
					placeholder="请输入配置编码"
				/>
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

			<template v-if="state.pageMode === 'detail'">
				<wd-form-item title="创建人" prop="createdUserName">
					{{ state.formData.createdUserName }}
				</wd-form-item>
				<wd-form-item title="创建时间" prop="createdUserName">
					{{ state.formData.createdTime }}
				</wd-form-item>
				<wd-form-item title="更新人" prop="updatedUserName">
					{{ state.formData.updatedUserName }}
				</wd-form-item>
				<wd-form-item title="更新时间" prop="updatedTime">
					{{ state.formData.updatedTime }}
				</wd-form-item>
			</template>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" :disabled="state.formDisabled" @click="handleConfirm">保存</wd-button>
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
import type { QueryConfigDetailOutput } from "@/api/services/Center/config/models/QueryConfigDetailOutput";

definePage({
	name: "DevConfigEdit",
	layout: "layout",
	authCode: ["Config:Detail", "Config:Edit"],
	style: {
		navigationBarTitleText: "配置",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<QueryConfigDetailOutput>({}),
	formSchema: useFormSchema<AddConfigInput>({
		configCode: { required: true, message: "请输入配置编码" },
		configName: { required: true, message: "请输入配置名称" },
		configValue: { required: true, message: "请输入配置值" },
	}),
	formDisabled: false,
	pageMode: withDefineType<IPageStateType>("detail"),
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
		case "detail":
			{
				state.formDisabled = true;
				const apiRes = await configApi.queryConfigDetail(state.configId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `配置详情 - ${apiRes.configName}` });
			}
			break;
		case "add":
			state.formDisabled = false;
			uni.setNavigationBarTitle({ title: "添加配置" });
			break;
		case "edit":
			{
				state.formDisabled = false;
				const apiRes = await configApi.queryConfigDetail(state.configId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `编辑配置 - ${apiRes.configName}` });
			}
			break;
	}
};

onLoad((options: { mode?: IPageStateType; configId?: string }) => {
	if (!options?.mode || ((options.mode === "edit" || options.mode === "detail") && !options?.configId)) {
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
