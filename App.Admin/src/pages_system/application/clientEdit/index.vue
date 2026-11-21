<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-cell title="应用" :value="state.appName" />
			<wd-form-item title="应用标识" prop="appKey">
				<wd-input v-model="state.formData.appKey" clearable :maxlength="50" placeholder="请输入应用标识" />
			</wd-form-item>
			<wd-form-item title="应用密钥" prop="appSecret">
				<wd-input v-model="state.formData.appSecret" clearable :maxlength="50" placeholder="请输入应用密钥" />
			</wd-form-item>
			<wd-form-item title="应用类型" prop="appType">
				<wd-checkbox-group v-model="appTypes" type="square">
					<wd-checkbox :name="AppEnvironmentEnum.Web">Web</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.Windows">Windows</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.Mac">Mac</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.H5">H5</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.Android">Android</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.HarmonyOS">鸿蒙</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.IOS">IOS</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.WeChatMiniProgram">微信小程序</wd-checkbox>
				</wd-checkbox-group>
			</wd-form-item>
			<wd-form-item title="环境类型" prop="environmentType">
				<FaRadioGroup name="EnvironmentTypeEnum" v-model="state.formData.environmentType" />
			</wd-form-item>
			<wd-form-item title="登录组件" prop="loginComponent">
				<wd-radio-group v-model="state.formData.loginComponent">
					<wd-radio value="">无</wd-radio>
					<wd-radio value="ClassicLogin">经典</wd-radio>
					<wd-radio value="ModernLogin">现代</wd-radio>
					<wd-radio value="SimpleLogin">简约</wd-radio>
					<wd-radio value="SplitLogin">分屏</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="请求加密" prop="requestEncipher">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.requestEncipher" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="请求超时" prop="requestTimeout">
				<wd-input v-model.number="state.formData.requestTimeout" type="number" placeholder="请输入请求超时时间">
					<template #suffix>毫秒</template>
				</wd-input>
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

			<wd-form-item v-if="appTypes.includes(AppEnvironmentEnum.IOS)" title="App Store ID" prop="appStoreId">
				<wd-input v-model="state.formData.appStoreId" clearable :maxlength="30" placeholder="请输入数字 ID" />
			</wd-form-item>

			<wd-cell-group title="支付相关">
				<FaMerchantSelect
					title="微信商户号"
					v-model="state.formData.weChatMerchantId"
					v-model:merchant-no="state.formData.weChatMerchantNo"
					prop="weChatMerchantId"
					:merchant-type="PaymentChannelEnum.WeChat"
				/>
				<FaMerchantSelect
					title="支付宝商户号"
					v-model="state.formData.alipayMerchantId"
					v-model:merchant-no="state.formData.alipayMerchantNo"
					prop="alipayMerchantId"
					:merchant-type="PaymentChannelEnum.Alipay"
				/>
			</wd-cell-group>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { EnvironmentTypeEnum } from "@/api/enums/EnvironmentTypeEnum";
import { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";
import { applicationClientApi } from "@/api/services/Center/applicationClient";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { useApp, usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddApplicationClientInput } from "@/api/services/Center/applicationClient/models/AddApplicationClientInput";
import type { EditApplicationClientInput } from "@/api/services/Center/applicationClient/models/EditApplicationClientInput";

definePage({
	name: "SystemApplicationClientEdit",
	authCode: ["AppClient:Add", "AppClient:Edit"],
	style: {
		navigationBarTitleText: "应用标识",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();
const appStore = useApp();

const formRef = ref<FormInstance>();

const appEnvironmentEnum = appStore.getDictionary("AppEnvironmentEnum").filter((item) => {
	const flag = BigInt(item.value);
	return (flag & (flag - 1n)) === 0n;
});

const state = reactive({
	loading: false,
	formData: withDefineType<EditApplicationClientInput & AddApplicationClientInput>({}),
	formSchema: useFormSchema<EditApplicationClientInput & AddApplicationClientInput>({
		appKey: { required: true, message: "请输入应用标识" },
		appStoreId: { validator: (value) => (!value || /^[0-9]+$/.test(value) ? undefined : "App Store ID 只能包含数字") },
		appType: { required: true, message: "请选择应用类型" },
		environmentType: { required: true, message: "请选择环境类型" },
		requestTimeout: {
			required: true,
			message: "请输入请求超时时间",
			validator: (value) =>
				Number.isInteger(value) && value >= 5000 && value <= 60000 ? undefined : "请求超时必须是 5000 到 60000 毫秒之间的整数",
		},
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	recordId: withDefineType<string>(),
	appId: withDefineType<string>(),
	appName: withDefineType<string>(),
});

const appTypes = computed({
	get: () => {
		const value = BigInt(state.formData.appType || 0n);
		return appEnvironmentEnum.filter((item) => (value & BigInt(item.value)) !== 0n).map((item) => String(item.value));
	},
	set: (values: string[]) => {
		state.formData.appType = values.length
			? (values.reduce((value, item) => value | BigInt(item), 0n).toString() as AppEnvironmentEnum)
			: undefined;
	},
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		// Radio 使用空字符串表示“无”，提交时转换为接口的空值。
		const formData = {
			...state.formData,
			loginComponent: state.formData.loginComponent === "" ? null : state.formData.loginComponent,
		};
		switch (state.pageMode) {
			case "add":
				await applicationClientApi.addApplicationClient(formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await applicationClientApi.editApplicationClient(formData);
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
				uni.setNavigationBarTitle({ title: "添加应用标识" });
				state.formData = {
					appId: state.appId,
					appType: AppEnvironmentEnum.Web,
					environmentType: EnvironmentTypeEnum.Production,
					requestTimeout: 60000,
					requestEncipher: true,
					loginComponent: "",
				};
				break;
			case "edit":
				{
					const apiRes = await applicationClientApi.queryApplicationClientDetail(state.recordId);
					state.formData = { ...apiRes, loginComponent: apiRes.loginComponent ?? "" };
					uni.setNavigationBarTitle({ title: `编辑应用标识 - ${apiRes.appKey}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; recordId?: string; appId?: string; appName?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.recordId) || (options.mode === "add" && !options?.appId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.recordId = options.recordId;
	state.appId = options.appId;
	state.appName = options.appName;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>
